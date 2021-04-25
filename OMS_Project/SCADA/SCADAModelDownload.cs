using Common.DataModel;
using Common.GDA;
using Common.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA
{
	public class SCADAModelDownload
	{
		const int iteratorCount = 256;

		List<long> insertedGids;
		List<long> updatedGids;
		List<long> deletedGids;

		public List<IdentifiedObject> Inserted { get; private set; }
		public List<IdentifiedObject> Updated { get; private set; }
		public List<long> Deleted { get; private set; }

		public SCADAModelDownload(List<long> inserted, List<long> updated, List<long> deleted)
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
			Dictionary<DMSType, List<ModelCode>> typeToPropertiesMapMeas = new Dictionary<DMSType, List<ModelCode>>(2) { { DMSType.Analog, typeToPropertiesMap[DMSType.Analog] }, { DMSType.Discrete, typeToPropertiesMap[DMSType.Discrete] } };

			List<IdentifiedObject> inserted = new List<IdentifiedObject>(insertedGids.Count);
			List<IdentifiedObject> updated = new List<IdentifiedObject>(updatedGids.Count);

			List<ResourceDescription> result;
			int iterator = nms.GetMultipleValues(insertedGids, typeToPropertiesMapMeas, true);

			do
			{
				result = nms.IteratorNext(iteratorCount, iterator, true);

				if(result == null)
					return false;

				foreach(ResourceDescription rd in result)
				{
					inserted.Add(IdentifiedObject.Create(rd, true));
				}
			}
			while(result.Count >= iteratorCount);

			nms.IteratorClose(iterator);
			iterator = nms.GetMultipleValues(updatedGids, typeToPropertiesMapMeas, true);

			do
			{
				result = nms.IteratorNext(iteratorCount, iterator, true);

				if(result == null)
					return false;

				foreach(ResourceDescription rd in result)
				{
					updated.Add(IdentifiedObject.Create(rd, true));
				}
			}
			while(result.Count >= iteratorCount);

			Inserted = inserted;
			Updated = updated;
			Deleted = deletedGids;

			return true;
		}
	}
}
