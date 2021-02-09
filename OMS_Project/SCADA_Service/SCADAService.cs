using Common.GDA;
using SCADA_Client.ViewModel.PointViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Service
{
    public class SCADAService : IDisposable
    {
        private ChannelFactory<INetworkModelGDAContract> factory;
        private INetworkModelGDAContract proxy;

        public SCADAService()
        {
            Console.WriteLine("Started!");
        }

        public void Dispose()
        {
            ProcessHandler.KillProcesses();
            Disconnect();
            Console.WriteLine("Disposed!");
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
            try
            {
                ConnectToNMS("net.tcp://localhost:11123/NMS/GDA/");
                Console.WriteLine("Connected to NMS.");
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }

            ImportSCADAModel(proxy);

            InitializeSCADAClient();
            InitializeSCADAServer();
        }

        private void InitializeSCADAClient()
        {
            Process client = new Process();
            string clientPath = Path.GetFullPath("../../../SCADA_Client/bin/Debug/SCADA_Client.exe");
            client.StartInfo.FileName = "SCADA_Client.exe";
            client.StartInfo.WorkingDirectory = Path.GetDirectoryName(clientPath);
            client.Start();
            ProcessHandler.ActiveProcesses.Add(client);
        }

        private void InitializeSCADAServer()
        {
            Process server = new Process();
            string serverPath = Path.GetFullPath("../../../MdbSim/ModbusSim.exe");
            server.StartInfo.FileName = serverPath;
            server.StartInfo.WorkingDirectory = Path.GetDirectoryName(serverPath);
            server.Start();
            ProcessHandler.ActiveProcesses.Add(server);
        }

        public void ImportSCADAModel(INetworkModelGDAContract proxy)
        {
            SCADAModel sm = new SCADAModel(proxy);
            sm.ImportModel();
        }

        private bool ConnectToNMS(string uri)
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

        private void Disconnect()
        {
            try
            {
                factory.Close();
            }
            catch { }
        }

    }
}
