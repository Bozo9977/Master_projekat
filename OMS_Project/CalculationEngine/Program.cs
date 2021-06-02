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

			Measurements.Instance = new Measurements();
			TopologyModel model = new TopologyModel();

			if(!model.ApplyUpdate(download))
				return;

			TopologyModel.Instance = model;

			ServiceHost host = new ServiceHost(typeof(CalculationEngineService));
			host.Open();

			foreach(ServiceEndpoint endpoint in host.Description.Endpoints)
				Console.WriteLine(endpoint.ListenUri);

			Client<IPublishing> pubClient = new Client<IPublishing>("publishingEndpoint");
			pubClient.Connect();

			pubClient.Call<bool>(pub =>
			{
				pub.Publish(new TopologyChanged());
				return true;
			}, out _);

			pubClient.Disconnect();

			Console.WriteLine("[Press Enter to stop]");
			Console.ReadLine();
			host.Close();
		}
	}
}
