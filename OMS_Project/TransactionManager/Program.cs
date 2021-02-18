using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace TransactionManager
{
	class Program
	{
		static void Main(string[] args)
		{
			ServiceHost host = new ServiceHost(typeof(TransactionManagerService));
			host.Open();

			Console.WriteLine(host.BaseAddresses[0].ToString());
			Console.WriteLine("[Press any key to stop the service]");
			Console.ReadKey();
			host.Close();
		}
	}
}
