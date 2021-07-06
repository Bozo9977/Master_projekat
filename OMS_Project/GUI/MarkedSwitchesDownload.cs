using Common.CalculationEngine;
using Common.WCF;
using System.Collections.Generic;

namespace GUI
{
	public class MarkedSwitchesDownload
	{
		public List<KeyValuePair<long, bool>> Data { get; private set; }

		public bool Download()
		{
			Client<ICalculationEngineServiceContract> client = new Client<ICalculationEngineServiceContract>("endpointCE");
			client.Connect();
			bool ok;

			if(!client.Call<bool>(ce => Get(ce), out ok) || !ok)
			{
				client.Disconnect();
				return false;
			}

			client.Disconnect();

			return true;
		}

		bool Get(ICalculationEngineServiceContract ce)
		{
			Data = ce.GetMarkedSwitches();
			return true;
		}
	}
}