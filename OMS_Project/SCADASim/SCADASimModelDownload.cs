using Common.DataModel;
using Common.GDA;
using Common.WCF;
using System.Collections.Generic;

namespace SCADASim
{
	public class SCADASimModelDownload
	{
		const int iteratorCount = 256;
		public Dictionary<long, Analog> Analogs { get; private set; }
		public Dictionary<long, Discrete> Discretes { get; private set; }

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
			Dictionary<long, Analog> analogs = new Dictionary<long, Analog>();
			Dictionary<long, Discrete> discretes = new Dictionary<long, Discrete>();

			Dictionary<DMSType, List<ModelCode>> typeToPropertiesMap = ModelResourcesDesc.GetTypeToPropertiesMap();

			List<ResourceDescription> result;
			int iterator = nms.GetExtentValues(DMSType.Analog, typeToPropertiesMap[DMSType.Analog], true);

			do
			{
				result = nms.IteratorNext(iteratorCount, iterator, true);

				if(result == null)
					return false;

				foreach(ResourceDescription rd in result)
				{
					analogs.Add(rd.Id, (Analog)IdentifiedObject.Create(rd, true));
				}
			}
			while(result.Count >= iteratorCount);

			nms.IteratorClose(iterator);
			iterator = nms.GetExtentValues(DMSType.Discrete, typeToPropertiesMap[DMSType.Discrete], true);

			do
			{
				result = nms.IteratorNext(iteratorCount, iterator, true);

				if(result == null)
					return false;

				foreach(ResourceDescription rd in result)
				{
					discretes.Add(rd.Id, (Discrete)IdentifiedObject.Create(rd, true));
				}
			}
			while(result.Count >= iteratorCount);

			Analogs = analogs;
			Discretes = discretes;

			return true;
		}
	}
}
