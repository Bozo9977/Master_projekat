using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Service
{
    public class SCADAService : IDisposable
    {
        public SCADAService()
        {
            //ImportScadaModel();
            Console.WriteLine("Started!");
        }

        public void Dispose()
        {
            ProcessHandler.KillProcesses();
            Console.WriteLine("Disposed!");
            GC.SuppressFinalize(this);
        }

        public void Start()
        {
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


    }
}
