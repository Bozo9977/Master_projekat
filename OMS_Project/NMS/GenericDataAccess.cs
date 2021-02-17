using Common.GDA;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace NMS
{
	class GenericDataAccess : INetworkModelGDAContract
	{
		static object updateLock = new object();
		static NetworkModel model = new NetworkModel(new NMSEFDatabase());
		static NetworkModel transactionModel = model;
		static ConcurrentDictionary<int, ResourceIterator> iterators = new ConcurrentDictionary<int, ResourceIterator>();
		static int iteratorId;

        public UpdateResult ApplyUpdate(Delta delta)
		{
			lock(updateLock)
			{
				transactionModel = new NetworkModel(model);
				Dictionary<long, long> mappings = transactionModel.ApplyUpdate(delta);

				if(mappings == null)
				{
					Interlocked.Exchange(ref transactionModel, model);
					return new UpdateResult(null, null, ResultType.Failure);
				}

				Interlocked.Exchange(ref model, transactionModel);
				return new UpdateResult(mappings, null, ResultType.Success);
			}
		}

		public int GetExtentValues(ModelCode entityType, List<ModelCode> propIds)
		{
			return AddIterator(model.GetExtentValues(entityType, propIds));
		}

		public int GetRelatedValues(long source, List<ModelCode> propIds, Association association)
		{
			return AddIterator(model.GetRelatedValues(source, propIds, association));
		}

		public ResourceDescription GetValues(long resourceId, List<ModelCode> propIds)
		{
			return model.GetValues(resourceId, propIds);
		}

		public bool IteratorClose(int id)
		{
			return RemoveIterator(id);
		}

		public List<ResourceDescription> IteratorNext(int n, int id)
		{
			ResourceIterator ri = GetIterator(id);

			if(ri == null)
				return null;

			return ri.Next(n, model);
		}

		public int IteratorResourcesLeft(int id)
		{
			ResourceIterator ri = GetIterator(id);

			if(ri == null)
				return -1;

			return ri.ResourcesLeft();
		}

		public int IteratorResourcesTotal(int id)
		{
			ResourceIterator ri = GetIterator(id);

			if(ri == null)
				return -1;

			return ri.ResourcesTotal();
		}

		public bool IteratorRewind(int id)
		{
			ResourceIterator ri = GetIterator(id);

			if(ri == null)
				return false;

			ri.Rewind();
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
			ResourceIterator ri = null;
			iterators.TryGetValue(id, out ri);
			return ri;
		}
	}
}
