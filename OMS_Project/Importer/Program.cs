using Common.GDA;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Importer
{
	class Program
	{
		const string menu = "1. Apply delta from CIMXML file\n" +
							"2. Quit\n";
		static ChannelFactory<INetworkModelGDAContract> factory;
		static INetworkModelGDAContract proxy;

		static void Main(string[] args)
		{
			while(true)
			{
				Console.WriteLine(menu);
				Console.Write(">>");

				switch(Console.ReadLine())
				{
					case "1":
						Console.Write("CIMXML file path: ");
						Delta delta;

						try
						{
							using(FileStream fs = File.Open(Console.ReadLine(), FileMode.Open))
							{
								delta = new Adapter().CreateDelta(fs);
							}
						}
						catch
						{
							delta = null;
						}

						if(delta == null)
						{
							Console.WriteLine("ERROR: Delta not created.");
							break;
						}
						
						try
						{
							Connect("net.tcp://localhost:11123/NMS/GDA/");
							UpdateResult result = proxy.ApplyUpdate(delta);
							Disconnect();
							Console.WriteLine(result.ToString());
						}
						catch(Exception e)
						{
							Console.WriteLine("ERROR: " + e.Message);
						}
						
						break;

					case "2":
						return;
				}

				Console.WriteLine();
			}
		}

		static bool Connect(string uri)
		{
			try
			{
				factory = new ChannelFactory<INetworkModelGDAContract>(new NetTcpBinding(), new EndpointAddress(new Uri(uri)));
				proxy = factory.CreateChannel();
			}
			catch
			{
				Disconnect();
				return false;
			}

			return true;
		}

		static void Disconnect()
		{
			try
			{
				factory.Close();
			}
			catch { }
		}
	}
}
