using Common.GDA;
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
		static readonly object scadaLock = new object();
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
				Dictionary<long, long> mappings;
				NetworkModel tModel;
				DuplexClient<ITransactionManager, ITransaction> client = new DuplexClient<ITransactionManager, ITransaction>("callbackEndpoint", this);

				client.Connect();

				if(!client.Call<bool>(tm => tm.StartEnlist(), out ok) || !ok)   //TM.StartEnlist()
				{
					client.Disconnect();
					return new UpdateResult(null, null, ResultType.Failure);
				}

				tModel = new NetworkModel(model);
				mappings = tModel.ApplyUpdate(delta);

				if(mappings == null)
				{
					client.Call<bool>(tm => tm.EndEnlist(false), out ok);   //TM.EndEnlist(false)
					client.Disconnect();
					return new UpdateResult(null, null, ResultType.Failure);
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
					return new UpdateResult(null, null, ResultType.Failure);
				}

				// Call SCADA
				Client<ISCADAServiceContract> scadaClient = new Client<ISCADAServiceContract>("SCADAEndpoint");
				scadaClient.Connect();

				UpdateResult okResult = new UpdateResult(null, null, ResultType.Success);

				lock (scadaLock)
                {
					scadaClient.Call<UpdateResult>(ss => ss.ApplyUpdate(), out okResult);
                }					

				

				//if(!SCADA.ApplyUpdate(affectedGIDs)) { ... }
				//if(!CE.ApplyUpdate(affectedGIDs)) { ... }

				if (!client.Call<bool>(tm => tm.EndEnlist(true), out ok) || !ok)   //TM.EndEnlist(true)
				{
					lock(modelLock)
					{
						transactionModel = model;
					}

					client.Disconnect();
					return new UpdateResult(null, null, ResultType.Failure);
				}

				client.Disconnect();
				scadaClient.Disconnect();

				lock (modelLock)
				{
					return model == tModel ? new UpdateResult(mappings, null, ResultType.Success) : new UpdateResult(null, null, ResultType.Failure);
				}
			}
		}

		public int GetExtentValues(DMSType entityType, List<ModelCode> propIds, bool transaction)
		{
			return AddIterator((transaction ? transactionModel : model).GetExtentValues(entityType, propIds));
		}

		public int GetRelatedValues(long source, List<ModelCode> propIds, Association association, bool transaction)
		{
			return AddIterator((transaction ? transactionModel : model).GetRelatedValues(source, propIds, association));
		}

		public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds, bool transaction)
		{
			return (transaction ? transactionModel : model).GetValues(resourceId, propIds);
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
			}

			return transactionModel.PersistUpdate();
		}

		public void Commit()
		{
			lock(modelLock)
			{
				model = transactionModel;
			}
		}

		public void Rollback()
		{
			transactionModel.RollbackUpdate();

			lock(modelLock)
			{
				transactionModel = model;
			}
		}
	}
}
