using Common.GDA;
using SCADA_Client.ViewModel.PointViewModels;
using SCADA_Common;
using SCADA_Common.Data;
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

        }


        private void ImportAnalog()
        {
            bool success;
            int numberOfResources = 1000;
            List<ModelCode> props = new List<ModelCode>();
            props.Add(ModelCode.ANALOG_MAXVALUE);
            props.Add(ModelCode.ANALOG_MINVALUE);
            props.Add(ModelCode.ANALOG_NORMALVALUE);
            props.Add(ModelCode.IDENTIFIEDOBJECT_GID);
            props.Add(ModelCode.IDENTIFIEDOBJECT_NAME);
            props.Add(ModelCode.MEASUREMENT_BASEADDRESS);
            props.Add(ModelCode.MEASUREMENT_DIRECTION);

            int iteratorId = proxy.GetExtentValues(ModelCode.ANALOG, props);
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

                        //Logger.LogDebug($"ANALOG measurement added to SCADA model [Gid: {gid}, Address: {pointItem.Address}]");
                    }
                }
                resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
            }
        }
    }
}
