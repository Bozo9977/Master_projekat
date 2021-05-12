using Common.GDA;
using Common.PubSub;
using Common.SCADA;
using Common.Transaction;
using Common.WCF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ServiceModel;
using System.Threading;

namespace NMS
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
	class GenericDataAccess : INetworkModelGDAContract, ITransaction
	{
		static readonly object updateLock = new object();
		static readonly object modelLock = new object();
		static NetworkModel model = new NetworkModel(new NMSEFDatabase());
		static NetworkModel transactionModel = model;
		static readonly ConcurrentDictionary<int, ResourceIterator> iterators = new ConcurrentDictionary<int, ResourceIterator>();
		static int iteratorId;

		public UpdateResult ApplyUpdate(Delta delta)
		{
			lock(updateLock)
			{
				bool ok;
				DuplexClient<ITransactionManager, ITransaction> client = new DuplexClient<ITransactionManager, ITransaction>("callbackEndpoint", this);
				client.Connect();

				if(!client.Call<bool>(tm => tm.StartEnlist(), out ok) || !ok)   //TM.StartEnlist()
				{
					client.Disconnect();
					return new UpdateResult(ResultType.Failure);
				}

				NetworkModel tModel = new NetworkModel(model);
				Tuple<Dictionary<long, long>, List<long>, List<long>> result = tModel.ApplyUpdate(delta);

				if(result == null)
				{
					client.Call<bool>(tm => tm.EndEnlist(false), out ok);   //TM.EndEnlist(false)
					client.Disconnect();
					return new UpdateResult(ResultType.Failure);
				}

				lock(modelLock)
				{
					transactionModel = tModel;
				}

				if(!client.Call<bool>(tm => tm.Enlist(), out ok) || !ok)   //TM.Enlist()
				{
					lock(modelLock)
					{
						transactionModel = model;
					}

					client.Call<bool>(tm => tm.EndEnlist(false), out ok);   //TM.EndEnlist(false)
					client.Disconnect();
					return new UpdateResult(ResultType.Failure);
				}

				Client<ISCADAServiceContract> scadaClient = new Client<ISCADAServiceContract>("SCADAEndpoint"); // Call SCADA
				scadaClient.Connect();

				if(!scadaClient.Call<bool>(scada => scada.ApplyUpdate(new List<long>(result.Item1.Values), result.Item2, result.Item3), out ok) || !ok)
				{
					scadaClient.Disconnect();

					lock(modelLock)
					{
						transactionModel = model;
					}

					client.Call<bool>(tm => tm.EndEnlist(false), out ok);   //TM.EndEnlist(false)
					client.Disconnect();
					return new UpdateResult(ResultType.Failure);
				}

				scadaClient.Disconnect();

				//if(!CE.ApplyUpdate(affectedGIDs)) { ... }

				if(!client.Call<bool>(tm => tm.EndEnlist(true), out ok) || !ok)   //TM.EndEnlist(true)
				{
					lock(modelLock)
					{
						transactionModel = model;
					}

					client.Disconnect();
					return new UpdateResult(ResultType.Failure);
				}

				client.Disconnect();

				bool success;

				lock(modelLock)
				{
					success = model == tModel;
				}

				if(success)
				{
					Client<IPublishing> pubClient = new Client<IPublishing>("publishingEndpoint");
					pubClient.Connect();

					pubClient.Call<bool>(pub =>
					{
						pub.Publish(new NetworkModelChanged());
						return true;
					}, out ok);

					pubClient.Disconnect();
				}

				return success ? new UpdateResult(ResultType.Success, null, result.Item1, result.Item2, result.Item3) : new UpdateResult(ResultType.Failure);
			}
		}

		public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds, bool transaction)
		{
			return (transaction ? transactionModel : model).GetValues(resourceId, propIds);
		}

		public int GetMultipleValues(List<long> resourceIds, Dictionary<DMSType, List<ModelCode>> typeToProps, bool transaction)
		{
			return AddIterator((transaction ? transactionModel : model).GetMultipleValues(resourceIds, typeToProps));
		}

		public int GetExtentValues(DMSType entityType, List<ModelCode> propIds, bool transaction)
		{
			return AddIterator((transaction ? transactionModel : model).GetExtentValues(entityType, propIds));
		}

		public int GetRelatedValues(long source, List<ModelCode> propIds, Association association, bool transaction)
		{
			return AddIterator((transaction ? transactionModel : model).GetRelatedValues(source, propIds, association));
		}

		public bool IteratorClose(int id)
		{
			return RemoveIterator(id);
		}

		public List<ResourceDescription> IteratorNext(int n, int id, bool transaction)
		{
			ResourceIterator ri = GetIterator(id);

			if(ri == null)
				return null;

			lock(ri)
			{
				return ri.Next(n, transaction ? transactionModel : model);
			}
		}

		public int IteratorResourcesLeft(int id)
		{
			ResourceIterator ri = GetIterator(id);

			if(ri == null)
				return -1;

			lock(ri)
			{
				return ri.ResourcesLeft();
			}
		}

		public int IteratorResourcesTotal(int id)
		{
			ResourceIterator ri = GetIterator(id);

			if(ri == null)
				return -1;

			lock(ri)
			{
				return ri.ResourcesTotal();
			}
		}

		public bool IteratorRewind(int id)
		{
			ResourceIterator ri = GetIterator(id);

			if(ri == null)
				return false;

			lock(ri)
			{
				ri.Rewind();
			}
			
			return true;
		}

		int AddIterator(ResourceIterator iterator)
		{
			int id = Interlocked.Increment(ref iteratorId);
			return iterators.TryAdd(id, iterator) ? id : -1;
		}

		bool RemoveIterator(int id)
		{
			return iterators.TryRemove(id, out _);
		}

		ResourceIterator GetIterator(int id)
		{
			ResourceIterator ri;
			iterators.TryGetValue(id, out ri);
			return ri;
		}

		public bool Prepare()
		{
			lock(modelLock)
			{
				if(model == transactionModel)
					return false;

				return transactionModel.PersistUpdate();
			}
		}

		public void Commit()
		{
			lock(modelLock)
			{
				model = transactionModel;
				model.CommitUpdate();
			}
		}

		public void Rollback()
		{
			lock(modelLock)
			{
				transactionModel.RollbackUpdate();
				transactionModel = model;
			}
		}
	}
}
