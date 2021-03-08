using Common.GDA;
using SCADA_Client.ViewModel.PointViewModels;
using SCADA_Common;
using SCADA_Common.DAO;
using SCADA_Common.Data;
using SCADA_Common.DB_Model;
using SCADA_Service.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Service
{
    public class SCADAModel
    {
        private Dictionary<long, ISCADAModelPointItem> scadaModel;
        private ConfigUpdater configUpdater;

        INetworkModelGDAContract proxy;

        private Dictionary<PointType, Dictionary<ushort, long>> addressToGidMap;

        public SCADAModel()
        {
            scadaModel = new Dictionary<long, ISCADAModelPointItem>();
        }

        public SCADAModel(INetworkModelGDAContract p)
        {
            scadaModel = new Dictionary<long, ISCADAModelPointItem>();
            proxy = p;
        }

        public Dictionary<long, ISCADAModelPointItem> ScadaModel
        {
            get { return scadaModel ?? (scadaModel = new Dictionary<long, ISCADAModelPointItem>()); }
        }


        public Dictionary<PointType, Dictionary<ushort, long>> AddressToGidMap
        {
            get
            {
                return addressToGidMap ?? (addressToGidMap = new Dictionary<PointType, Dictionary<ushort, long>>()
                {
                    { PointType.ANALOG_INPUT,   new Dictionary<ushort, long>()  },
                    { PointType.ANALOG_OUTPUT,  new Dictionary<ushort, long>()  },
                    { PointType.DIGITAL_INPUT,  new Dictionary<ushort, long>()  },
                    { PointType.DIGITAL_OUTPUT, new Dictionary<ushort, long>()  },
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
            ////
            RepoAccess<PointItemDB> ra = new SCADA_Common.DAO.RepoAccess<PointItemDB>();
            foreach(var item in scadaModel.Values)
            {
                try
                {
                    if (ra.GetAll().SingleOrDefault(x => x.Gid == item.Gid) == null)
                    {
                        ra.Insert(new PointItemDB()
                        {
                            Address = item.Address,
                            Gid = item.Gid,
                            Alarm = false,
                            Name = item.Name,
                            RegisterType = item.RegisterType
                        });
                    }
                    else
                        continue;
                }
                catch(Exception e)
                {
                    Console.WriteLine(e.Message);
                }
                
            }

            ////
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
                    }
                }
                resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
            }
        }
    }
}
