using Common.DataModel;
using Common.GDA;
using Common.WCF;
using System.Collections.Generic;

namespace CalculationEngine
{
	public class TopologyModelDownload
	{
		const int iteratorCount = 256;

		List<long> insertedGids;
		List<long> updatedGids;
		List<long> deletedGids;

		public List<IdentifiedObject> Inserted { get; private set; }
		public List<IdentifiedObject> Updated { get; private set; }
		public List<long> Deleted { get; private set; }

		public TopologyModelDownload()
		{ }

		public TopologyModelDownload(List<long> inserted, List<long> updated, List<long> deleted)
		{
			insertedGids = inserted;
			updatedGids = updated;
			deletedGids = deleted;
		}

		public bool Download()
		{
			Client<INetworkModelGDAContract> client = new Client<INetworkModelGDAContract>("endpointNMS");

			if(!client.Connect())
				return false;

			bool success;

			if(!client.Call<bool>(Get, out success) || !success)
				return false;

			return true;
		}

		bool Get(INetworkModelGDAContract nms)
		{
			Dictionary<DMSType, List<ModelCode>> typeToPropertiesMap = ModelResourcesDesc.GetTypeToPropertiesMap();

			if(insertedGids == null || updatedGids == null || deletedGids == null)
			{
				List<IdentifiedObject> inserted = new List<IdentifiedObject>();
				DMSType[] types = ModelResourcesDesc.TypeIdsInInsertOrder;

				foreach(DMSType type in types)
				{
					int iterator = nms.GetExtentValues(type, typeToPropertiesMap[type], false);

					if(iterator < 0)
						return false;

					List<ResourceDescription> result;

					do
					{
						result = nms.IteratorNext(iteratorCount, iterator, false);

						if(result == null)
							return false;

						foreach(ResourceDescription rd in result)
						{
							if(rd == null)
								continue;

							inserted.Add(IdentifiedObject.Create(rd, true));
						}
					}
					while(result.Count >= iteratorCount);

					nms.IteratorClose(iterator);

					Inserted = inserted;
					Updated = new List<IdentifiedObject>(0);
					Deleted = new List<long>(0);
				}
			}
			else
			{
				List<IdentifiedObject> inserted = new List<IdentifiedObject>(insertedGids.Count);
				List<IdentifiedObject> updated = new List<IdentifiedObject>(updatedGids.Count);

				List<ResourceDescription> result;
				int iterator = nms.GetMultipleValues(insertedGids, typeToPropertiesMap, true);

				do
				{
					result = nms.IteratorNext(iteratorCount, iterator, true);

					if(result == null)
						return false;

					foreach(ResourceDescription rd in result)
					{
						if(rd == null)
							continue;

						inserted.Add(IdentifiedObject.Create(rd, true));
					}
				}
				while(result.Count >= iteratorCount);

				nms.IteratorClose(iterator);
				iterator = nms.GetMultipleValues(updatedGids, typeToPropertiesMap, true);

				do
				{
					result = nms.IteratorNext(iteratorCount, iterator, true);

					if(result == null)
						return false;

					foreach(ResourceDescription rd in result)
					{
						if(rd == null)
							continue;

						updated.Add(IdentifiedObject.Create(rd, true));
					}
				}
				while(result.Count >= iteratorCount);

				nms.IteratorClose(iterator);

				Inserted = inserted;
				Updated = updated;
				Deleted = deletedGids;
			}

			return true;
		}
	}
}