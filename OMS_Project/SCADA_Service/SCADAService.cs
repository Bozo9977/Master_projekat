using Common.DataModel;
using Common.GDA;
using Common.SCADA;
using Common.Transaction;
using Common.WCF;
using Messages.Commands;
using NServiceBus;
using SCADA_Client.ViewModel.PointViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCADA_Service
{
    [CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
    public class SCADAService : IDisposable, ITransaction, ISCADAServiceContract
    {
        private ChannelFactory<INetworkModelGDAContract> factory;
        private INetworkModelGDAContract proxy;

        //novo
        static readonly object updateLock = new object();
        static readonly object modelLock = new object();
        static SCADAModel scadaModel = new SCADAModel();
        static SCADAModel transactionModel = scadaModel;

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
            scadaModel = new SCADAModel(proxy);
            scadaModel.ImportModel();
        }

        //MASIVNA METODA
        /*public UpdateResult ImportSCADAModel(INetworkModelGDAContract proxy)
        {
            lock (updateLock)
            {
                bool ok;
                SCADAModel tModel;
                scadaModel = new SCADAModel(proxy);
                DuplexClient<ITransactionManager, ITransaction> client = new DuplexClient<ITransactionManager, ITransaction>("callbackEndpointScada", this);

                client.Connect();

                if (!client.Call<bool>(tm => tm.StartEnlist(), out ok) || !ok)   //TM.StartEnlist()
                {
                    client.Disconnect();
                    return new UpdateResult(null, null, ResultType.Failure);
                }

                tModel = new SCADAModel(scadaModel);
                tModel.ImportModel();

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

                //if(!SCADA.ApplyUpdate(affectedGIDs)) { ... }
                //if(!CE.ApplyUpdate(affectedGIDs)) { ... }

                if (!client.Call<bool>(tm => tm.EndEnlist(true), out ok) || !ok)   //TM.EndEnlist(true)
                {
                    lock (modelLock)
                    {
                        transactionModel = scadaModel;
                    }

                    client.Disconnect();
                    return new UpdateResult(null, null, ResultType.Failure);
                }

                client.Disconnect();

                lock (modelLock)
                {
                    return scadaModel == tModel ? new UpdateResult(null, null, ResultType.Success) : new UpdateResult(null, null, ResultType.Failure);
                }
            }
        }*/


        public UpdateResult ApplyUpdate(List<IdentifiedObject> inserted, List<IdentifiedObject> updated, List<IdentifiedObject> deleted)
        {
            throw new NotImplementedException();
        }


        public bool Prepare()
        {
            lock (modelLock)
            {
                if (scadaModel == transactionModel)
                    return false;
            }

            // Srediti bazu
            //return transactionModel.PersistUpdate();
            return true;
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
            // Srediti bazu
            //transactionModel.RollbackUpdate();

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
    }
}
