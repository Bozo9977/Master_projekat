using Common.CalculationEngine;
using Common.WCF;
using System;
using System.Collections.Generic;

namespace GUI
{
	public class TopologyDownload
	{
		public List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> Topology { get; private set; }

		public bool Download()
		{
			Client<ICalculationEngineServiceContract> client = new Client<ICalculationEngineServiceContract>("endpointCE");
			client.Connect();
			bool ok;

			if(!client.Call<bool>((ce) => Get(ce), out ok) || !ok)
				return false;

			return true;
		}

		bool Get(ICalculationEngineServiceContract ce)
		{
			Topology = ce.GetLineEnergization();
			return true;
		}
	}
}