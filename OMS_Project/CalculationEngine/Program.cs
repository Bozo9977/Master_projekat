using Common;
using Common.PubSub;
using Common.WCF;
using System;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Threading;

namespace CalculationEngine
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.WriteLine("Waiting for NMS, press [Enter] to quit.");
			TopologyModelDownload download = new TopologyModelDownload();

			while(!download.Download())
			{
				while(Console.KeyAvailable)
				{
					if(Console.ReadKey().Key == ConsoleKey.Enter)
						return;
				}

				Thread.Sleep(1000);
			}

			Console.WriteLine("Downloaded network model from NMS.");

			TopologyModel model = new TopologyModel(DailyLoadProfile.LoadFromXML("Daily_load_profiles.xml"));

			if(!model.ApplyUpdate(download))
				return;

			TopologyModel.Instance = model;

			ServiceHost host = new ServiceHost(typeof(CalculationEngineService));
			host.Open();

			foreach(ServiceEndpoint endpoint in host.Description.Endpoints)
				Console.WriteLine(endpoint.ListenUri);

			model.DownloadMeasurements(null);

			Console.WriteLine("[Press Enter to stop]");
			Console.ReadLine();
			host.Close();
		}
	}
}
