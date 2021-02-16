using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.ServiceModel.Description;
using System.Text;
using System.Threading.Tasks;
using TransactionManager;

namespace NMS
{
    public class NetworkModelService : IDisposable
    {

        NetworkModel model = null;
        List<ServiceHost> hosts = null;

        public NetworkModelService()
        {
            model = new NetworkModel();
            GenericDataAccess.Model = model;

            InitializeHosts();
        }


        public void Start()
        {
            StartHosts();
        }

        public void Dispose()
        {
            CloseHosts();
            GC.SuppressFinalize(this);
        }



        public void InitializeHosts()
        {
            hosts = new List<ServiceHost>();
            var binding = new NetTcpBinding();
            binding.CloseTimeout = TimeSpan.FromMinutes(10);
            binding.OpenTimeout = TimeSpan.FromMinutes(10);
            binding.ReceiveTimeout = TimeSpan.FromMinutes(10);
            binding.SendTimeout = TimeSpan.FromMinutes(10);
            binding.TransactionFlow = true;

            ServiceHost host = new ServiceHost(typeof(NetworkModelTransactionService));
            host.Description.Name = "NetworkModelTransactionService";
            host.AddServiceEndpoint(typeof(ITransaction), binding, new Uri("net.tcp://localhost:8018/NetworkModelTransactionService"));

            host.Description.Behaviors.Remove(typeof(ServiceDebugBehavior));
            host.Description.Behaviors.Add(new ServiceDebugBehavior() { IncludeExceptionDetailInFaults = true });

            hosts.Add(host);

        }

        public void StartHosts()
        {
            if(hosts == null || hosts.Count == 0)
            {
                throw new Exception("Hosts in Network Model Service can't be opened because they aren't initialized.");
            }

            string msg = "";

            foreach(ServiceHost host in hosts)
            {
                host.Open();

                msg = $"WCF service {host.Description.Name} is ready";
                Console.WriteLine(msg);
                Console.WriteLine("\n");
                
            }

            msg = "Network Model Service started.";
            Console.WriteLine($"\n{msg}");
        }

        public void CloseHosts()
        {
            if(hosts==null || hosts.Count == 0)
            {
                throw new Exception("Hosts in Network Model Service can't be opened because they aren't initialized.");
            }

            foreach(ServiceHost host in hosts)
            {
                host.Close();
            }

            Console.WriteLine("\n\nHosts in Network Model Service are closed.");
        }
    }
}
