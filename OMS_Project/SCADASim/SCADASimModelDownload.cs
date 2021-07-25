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
		public Dictionary<long, Recloser> Reclosers { get; private set; }
		public Dictionary<long, Terminal> Terminals { get; private set; }
		public Dictionary<long, EnergyConsumer> EnergyConsumers { get; private set; }

		public bool Download()
		{
			Client<INetworkModelGDAContract> client = new Client<INetworkModelGDAContract>("endpointNMS");

			if(!client.Connect())
				return false;

			bool success;

			if(!client.Call<bool>(Get, out success) || !success)
			{
				client.Disconnect();
				return false;
			}

			client.Disconnect();

			return true;
		}

		bool Get(INetworkModelGDAContract nms)
		{
			Dictionary<long, Analog> analogs = new Dictionary<long, Analog>();
			Dictionary<long, Discrete> discretes = new Dictionary<long, Discrete>();
			Dictionary<long, Recloser> reclosers = new Dictionary<long, Recloser>();
			Dictionary<long, Terminal> terminals = new Dictionary<long, Terminal>();
			Dictionary<long, EnergyConsumer> energyConsumers = new Dictionary<long, EnergyConsumer>();

			Dictionary<DMSType, List<ModelCode>> typeToPropertiesMap = ModelResourcesDesc.GetTypeToPropertiesMap();

			List<ResourceDescription> result;
			int iterator = nms.GetExtentValues(DMSType.Analog, typeToPropertiesMap[DMSType.Analog], false);

			do
			{
				result = nms.IteratorNext(iteratorCount, iterator, false);

				if(result == null)
					return false;

				foreach(ResourceDescription rd in result)
				{
					analogs.Add(rd.Id, (Analog)IdentifiedObject.Create(rd, true));
				}
			}
			while(result.Count >= iteratorCount);

			nms.IteratorClose(iterator);
			iterator = nms.GetExtentValues(DMSType.Discrete, typeToPropertiesMap[DMSType.Discrete], false);

			do
			{
				result = nms.IteratorNext(iteratorCount, iterator, false);

				if(result == null)
					return false;

				foreach(ResourceDescription rd in result)
				{
					discretes.Add(rd.Id, (Discrete)IdentifiedObject.Create(rd, true));
				}
			}
			while(result.Count >= iteratorCount);

			nms.IteratorClose(iterator);
			iterator = nms.GetExtentValues(DMSType.EnergyConsumer, typeToPropertiesMap[DMSType.EnergyConsumer], false);

			do
			{
				result = nms.IteratorNext(iteratorCount, iterator, false);

				if(result == null)
					return false;

				foreach(ResourceDescription rd in result)
				{
					energyConsumers.Add(rd.Id, (EnergyConsumer)IdentifiedObject.Create(rd, true));
				}
			}
			while(result.Count >= iteratorCount);

			nms.IteratorClose(iterator);
			iterator = nms.GetExtentValues(DMSType.Recloser, typeToPropertiesMap[DMSType.Recloser], false);

			do
			{
				result = nms.IteratorNext(iteratorCount, iterator, false);

				if(result == null)
					return false;

				foreach(ResourceDescription rd in result)
				{
					reclosers.Add(rd.Id, (Recloser)IdentifiedObject.Create(rd, true));
				}
			}
			while(result.Count >= iteratorCount);

			nms.IteratorClose(iterator);

			List<long> terminalGIDs = new List<long>(reclosers.Count * 2);

			foreach(Recloser r in reclosers.Values)
			{
				for(int i = 0; i < r.Terminals.Count; ++i)
				{
					terminalGIDs.Add(r.Terminals[i]);
				}
			}

			iterator = nms.GetMultipleValues(terminalGIDs, typeToPropertiesMap, false);

			do
			{
				result = nms.IteratorNext(iteratorCount, iterator, false);

				if(result == null)
					return false;

				foreach(ResourceDescription rd in result)
				{
					if(terminals.ContainsKey(rd.Id))
						continue;

					terminals.Add(rd.Id, (Terminal)IdentifiedObject.Create(rd, true));
				}
			}
			while(result.Count >= iteratorCount);

			nms.IteratorClose(iterator);

			Analogs = analogs;
			Discretes = discretes;
			Reclosers = reclosers;
			Terminals = terminals;
			EnergyConsumers = energyConsumers;

			return true;
		}
	}
}
