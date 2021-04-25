using Common.GDA;
using Common.WCF;
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
		const string menu =	"1. Apply delta from CIMXML file\n" +
									"2. Clear network model\n" +
									"3. Quit\n";
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
					{
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

						Client<INetworkModelGDAContract> client = new Client<INetworkModelGDAContract>("endpointNMS");
						client.Connect();
						UpdateResult result;
						bool ok = client.Call<UpdateResult>(nms => nms.ApplyUpdate(delta), out result);
						client.Disconnect();

						if(!ok || result == null)
						{
							Console.WriteLine("ERROR: Update failed.");
							break;
						}

						Console.Write(result.ToString());
					}
					break;

					case "2":
					{
						Client<INetworkModelGDAContract> client = new Client<INetworkModelGDAContract>("endpointNMS");
						client.Connect();
						UpdateResult result;
						bool ok = client.Call<UpdateResult>(nms =>
						{
							Array types = Enum.GetValues(typeof(DMSType));
							List<long> gids = new List<long>();
							List<ModelCode> props = new List<ModelCode>();
							Delta delta = new Delta();

							foreach(DMSType type in types)
							{
								int iterator = nms.GetExtentValues(type, props, false);

								if(iterator < 0)
									return null;

								List<ResourceDescription> rds;

								do
								{
									rds = nms.IteratorNext(512, iterator, false);

									if(rds == null)
										return null;

									delta.DeleteOperations.AddRange(rds);
								}
								while(rds.Count > 0);

								nms.IteratorClose(iterator);
							}

							return nms.ApplyUpdate(delta);
						}, out result);
						client.Disconnect();

						if(!ok || result == null)
						{
							Console.WriteLine("ERROR: Clear failed.");
							break;
						}

						Console.Write(result.ToString());
					}
					break;

					case "3":
						return;
				}

				Console.WriteLine();
			}
		}
	}
}
