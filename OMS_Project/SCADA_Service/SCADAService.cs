using Common.DataModel;
using Common.GDA;
using Common.SCADA;
using Common.Transaction;
using Common.WCF;
using SCADA_Common.DAO;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceModel;

namespace SCADA_Service
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class SCADAService :  ITransaction, ISCADAServiceContract
    {
        private ChannelFactory<INetworkModelGDAContract> factory;
        private INetworkModelGDAContract proxy;
        DuplexClient<ITransactionManager, ITransaction> client;
        private ServiceHost scadaHost;

        //novo
        static readonly object updateLock = new object();
        static readonly object modelLock = new object();
        static SCADAModel scadaModel = new SCADAModel();
        static SCADAModel transactionModel = scadaModel;

        public SCADAService()
        {
            scadaHost = new ServiceHost(typeof(SCADAService));
            Console.WriteLine("Started!");
        }

        public void Start()
        {
            try
            {
                scadaHost.Open();
                ConnectToNMS("net.tcp://localhost:11123/NMS/GDA/");
                Console.WriteLine("Connected to NMS.");
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
            }

            Console.WriteLine("F1 Import SCADA model from NMS");
            Console.WriteLine("F2 Import SCADA model from DB");
            Console.Write(">> ");

            ConsoleKeyInfo key =  Console.ReadKey();

            switch(key.Key)
            {
                case ConsoleKey.F1:
                    ImportSCADAModel(proxy);
                    break;
                case ConsoleKey.F2:
                    ImportSCADAModelFromDB();
                    break;
                default:
                    break;
            }
           
            
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
            scadaModel = new SCADAModel(proxy);
            scadaModel.ImportModel();
        }

        public void ImportSCADAModelFromDB()
        {
            scadaModel.ImportModelFromDB();
        }

        public UpdateResult ApplyUpdate()
		{
			Console.WriteLine("Connection established.");

            ConnectToNMS("net.tcp://localhost:11123/NMS/GDA/");
            bool ok;
            SCADAModel tModel;
            scadaModel = new SCADAModel(proxy);
            scadaModel.ImportModelFromDB();

            client = new DuplexClient<ITransactionManager, ITransaction>("callbackEndpointScada", this);
            client.Connect();

            tModel = new SCADAModel(scadaModel);
            tModel.ImportTransactionModel();

            if (tModel == null || tModel.ScadaModel == null)
            {
                client.Call<bool>(tm => tm.EndEnlist(false), out ok);   //TM.EndEnlist(false)
                client.Disconnect();
                return new UpdateResult(null, null, ResultType.Failure);
            }

            lock (modelLock)
            {
                transactionModel = tModel;
            }

            if (!client.Call<bool>(tm => tm.Enlist(), out ok) || !ok)   //TM.Enlist()
            {
                lock (modelLock)
                {
                    transactionModel = scadaModel;
                }

                client.Call<bool>(tm => tm.EndEnlist(false), out ok);   //TM.EndEnlist(false)
                client.Disconnect();
                return new UpdateResult(null, null, ResultType.Failure);
            }

            lock (modelLock)
            {
                return scadaModel == tModel ? new UpdateResult(null, null, ResultType.Success) : new UpdateResult(null, null, ResultType.Failure);
            }
		}


        public bool Prepare()
        {
            lock (modelLock)
            {
                if (scadaModel == transactionModel)
                    return false;
            }

            return transactionModel.PersistUpdate();
        }

        public void Commit()
        {
            lock (modelLock)
            {
                scadaModel = transactionModel;
            }
        }

        public void Rollback()
        {
            transactionModel.RollbackUpdate();

            lock (modelLock)
            {
                transactionModel = scadaModel;
            }
        }

        private bool ConnectToNMS(string uri)
        {
            //Disconnect();

            try
            {
                factory = new ChannelFactory<INetworkModelGDAContract>(new NetTcpBinding() { TransferMode = TransferMode.Streamed, MaxReceivedMessageSize = 2147483647 }, new EndpointAddress(new Uri(uri)));
                proxy = factory.CreateChannel();
            }
            catch (Exception e)
            {
                Console.WriteLine("ERROR: " + e.Message);
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

            proxy = null;
            factory = null;
        }

        public void CloseSCADA()
        {
            scadaHost.Close();
            ProcessHandler.KillProcesses();
            Disconnect();
            client.Disconnect();
            Console.WriteLine("SCADA Closed!");
        }
	}
}
