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
		public Dictionary<DMSType, Dictionary<long, IdentifiedObject>> Containers { get; private set; }

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

			DMSType[] types = new DMSType[] { DMSType.Analog, DMSType.Discrete };
			Dictionary<DMSType, Dictionary<long, IdentifiedObject>> containers = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>(types.Length);

			foreach(DMSType type in types)
			{
				Dictionary<long, IdentifiedObject> container = new Dictionary<long, IdentifiedObject>();

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
						IdentifiedObject io = rd == null ? null : IdentifiedObject.Create(rd, true);
						container.Add(io.GID, io);
					}
				}
				while(result.Count >= iteratorCount);

				nms.IteratorClose(iterator);
				containers.Add(type, container);
			}

			Containers = containers;
			return true;
		}
	}
}
