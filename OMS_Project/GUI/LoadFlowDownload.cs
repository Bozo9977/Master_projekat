using Common.CalculationEngine;
using Common.WCF;
using System.Collections.Generic;

namespace GUI
{
	public class LoadFlowDownload
	{
		List<KeyValuePair<long, LoadFlowResult>> data;
		public IReadOnlyList<KeyValuePair<long, LoadFlowResult>> Data { get { return data; } }

		public bool Download()
		{
			Client<ICalculationEngineServiceContract> client = new Client<ICalculationEngineServiceContract>("endpointCE");
			client.Connect();
			bool ok;

			if(!client.Call<bool>((ce) => Get(ce), out ok) || !ok)
			{
				client.Disconnect();
				return false;
			}

			client.Disconnect();

			return true;
		}

		bool Get(ICalculationEngineServiceContract ce)
		{
			data = ce.GetLoadFlowResults();
			return true;
		}
	}
}