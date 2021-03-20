using Common.GDA;
using SCADA_Common;
using SCADA_Common.DAO;
using SCADA_Common.Data;
using SCADA_Common.DB_Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace SCADA_Service
{
    public class SCADAModel
    {
        private Dictionary<long, ISCADAModelPointItem> scadaModel;
        private ConfigUpdater configUpdater;
        readonly ReaderWriterLockSlim rwLock;

        INetworkModelGDAContract proxy;

        private Dictionary<PointType, Dictionary<short, long>> addressToGidMap;

        public SCADAModel()
        {
            scadaModel = new Dictionary<long, ISCADAModelPointItem>();
        }

        public SCADAModel(SCADAModel model)
        {
            foreach (var point in model.scadaModel)
            {
                scadaModel.Add(point.Key, point.Value);
            }
            proxy = model.proxy;
        }

        public SCADAModel(INetworkModelGDAContract p)
        {
            scadaModel = new Dictionary<long, ISCADAModelPointItem>();
            proxy = p;
        }

        public Dictionary<long, ISCADAModelPointItem> ScadaModel
        {
            get { return scadaModel ?? (scadaModel = new Dictionary<long, ISCADAModelPointItem>()); }
            set { scadaModel = value; }
        }


        public Dictionary<PointType, Dictionary<short, long>> AddressToGidMap
        {
            get
            {
                return addressToGidMap ?? (addressToGidMap = new Dictionary<PointType, Dictionary<short, long>>()
                {
                    { PointType.ANALOG_INPUT,   new Dictionary<short, long>()  },
                    { PointType.ANALOG_OUTPUT,  new Dictionary<short, long>()  },
                    { PointType.DIGITAL_INPUT,  new Dictionary<short, long>()  },
                    { PointType.DIGITAL_OUTPUT, new Dictionary<short, long>()  },
                });
            }
        }


        public void ImportModel()
        {
            Console.WriteLine("Importing analog values...");
            ImportAnalog();
            Console.WriteLine("Analog finished!");

            Console.WriteLine("Importing discrete values...");
            ImportDiscrete();
            Console.WriteLine("Discrete finished!");
            
            configUpdater = new ConfigUpdater();
            configUpdater.UpdateServerConfigFile(ScadaModel);
            configUpdater.UpdateClientConfigFile(ScadaModel);
        }

        public void ImportModelFromDB()
        {
            using (ScadaDB scadaDB = new ScadaDB())
            {
                ScadaModel = scadaDB.GetModel();
            }

            configUpdater = new ConfigUpdater();
            configUpdater.UpdateServerConfigFile(ScadaModel);
            configUpdater.UpdateClientConfigFile(ScadaModel);
        }

        private void ImportAnalog()
        {
            int numberOfResources = 1000;
            List<ModelCode> props = new List<ModelCode>();
            props.Add(ModelCode.ANALOG_MAXVALUE);
            props.Add(ModelCode.ANALOG_MINVALUE);
            props.Add(ModelCode.ANALOG_NORMALVALUE);
            props.Add(ModelCode.IDENTIFIEDOBJECT_GID);
            props.Add(ModelCode.IDENTIFIEDOBJECT_NAME);
            props.Add(ModelCode.MEASUREMENT_BASEADDRESS);
            props.Add(ModelCode.MEASUREMENT_DIRECTION);

            int iteratorId = proxy.GetExtentValues(DMSType.Analog, props, false);
            int resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

            RepoAccess<PointItemDB> ra = new SCADA_Common.DAO.RepoAccess<PointItemDB>();

            while (resourcesLeft > 0)
            {
                List<ResourceDescription> rds = proxy.IteratorNext(numberOfResources, iteratorId);
                for (int i = 0; i < rds.Count; i++)
                {
                    if (rds[i] != null)
                    {
                        long gid = rds[i].Id;
                        //ModelCode type = modelResourceDesc.GetModelCodeFromId(gid);
                        ISCADAModelPointItem pointItem = new AnalogSCADAModelPointItem(rds[i].Properties.Values.ToList(), ModelCode.ANALOG);
                        ScadaModel.Add(rds[i].Id, pointItem);
                        AddressToGidMap[pointItem.RegisterType].Add(pointItem.Address, rds[i].Id);
                        WriteAnalogIntoDB((AnalogSCADAModelPointItem)pointItem);
                    }

                }
                resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
            }
        }

        private void ImportDiscrete()
        {
            int numberOfResources = 1000;
            List<ModelCode> props = new List<ModelCode>();
            props.Add(ModelCode.DISCRETE_MAXVALUE);
            props.Add(ModelCode.DISCRETE_MINVALUE);
            props.Add(ModelCode.DISCRETE_NORMALVALUE);
            props.Add(ModelCode.IDENTIFIEDOBJECT_GID);
            props.Add(ModelCode.IDENTIFIEDOBJECT_NAME);
            props.Add(ModelCode.MEASUREMENT_BASEADDRESS);
            props.Add(ModelCode.MEASUREMENT_DIRECTION);

            int iteratorId = proxy.GetExtentValues(DMSType.Discrete, props, false);
            int resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

            while (resourcesLeft > 0)
            {
                List<ResourceDescription> rds = proxy.IteratorNext(numberOfResources, iteratorId);
                for (int i = 0; i < rds.Count; i++)
                {
                    if (rds[i] != null)
                    {
                        long gid = rds[i].Id;
                        ISCADAModelPointItem pointItem = new DiscreteSCADAModelPointItem(rds[i].Properties.Values.ToList(), ModelCode.DISCRETE);
                        ScadaModel.Add(gid, pointItem);
                        AddressToGidMap[pointItem.RegisterType].Add(pointItem.Address, gid);
                        WriteDiscreteIntoDB((DiscreteSCADAModelPointItem)pointItem);
                    }
                }
                resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
            }
        }

        private void WriteAnalogIntoDB(AnalogSCADAModelPointItem point)
        {
            RepoAccess<AnalogPointItemDB> ra = new RepoAccess<AnalogPointItemDB>();


            if (ra.GetAll().SingleOrDefault(x => x.Gid == point.Gid) == null)
            {
                ra.Insert(point.ToDBEntity()) ;
            }
        }

        private void WriteDiscreteIntoDB(DiscreteSCADAModelPointItem point)
        {
            RepoAccess<DiscretePointItemDB> ra = new RepoAccess<DiscretePointItemDB>();


            if (ra.GetAll().SingleOrDefault(x => x.Gid == point.Gid) == null)
            {
                ra.Insert(point.ToDBEntity());
            }
        }

        // Srediti bazu prvo
        /*public bool PersistUpdate()
        {
            rwLock.EnterReadLock();

            try
            {
                return db != null && inserted != null && db.PersistDelta(inserted, updatedNew, deleted);
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }

        public bool RollbackUpdate()
        {
            rwLock.EnterReadLock();

            try
            {
                return db != null && inserted != null && db.RollbackDelta(inserted, updatedOld, deleted);
            }
            finally
            {
                rwLock.ExitReadLock();
            }
        }*/
    }
}
