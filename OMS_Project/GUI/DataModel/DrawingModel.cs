using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.DataModel
{
    public class DrawingModel
    {
        public Dictionary<DMSType, Dictionary<long, IdentifiedObject>> CurrentModel { get; set; }
        private INetworkModelGDAContract proxy;

        public DrawingModel()
        {
            CurrentModel = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>();
            CurrentModel.Add(DMSType.Analog, new Dictionary<long, IdentifiedObject>());
            CurrentModel.Add(DMSType.Discrete, new Dictionary<long, IdentifiedObject>());

        }

        public DrawingModel(INetworkModelGDAContract proxy)
        {
            CurrentModel = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>();
            CurrentModel.Add(DMSType.Analog, new Dictionary<long, IdentifiedObject>());
            CurrentModel.Add(DMSType.Discrete, new Dictionary<long, IdentifiedObject>());
            this.proxy = proxy;
        }

        public void ImportModel()
        {
            ImportAnalog();
            ImportDiscrete();
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

            int iteratorId = proxy.GetExtentValues(ModelCode.ANALOG, props, false);
            int resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

            while (resourcesLeft > 0)
            {
                List<ResourceDescription> rds = proxy.IteratorNext(numberOfResources, iteratorId);
                for (int i = 0; i < rds.Count; i++)
                {
                    if (rds[i] != null)
                    {
                        long gid = rds[i].Id;
                        Analog analog = new Analog(rds[i].Properties.Values.ToList(), ModelCode.ANALOG);
                        CurrentModel[DMSType.Analog].Add(rds[i].Id, analog);
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

            int iteratorId = proxy.GetExtentValues(ModelCode.DISCRETE, props, false);
            int resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);

            while (resourcesLeft > 0)
            {
                List<ResourceDescription> rds = proxy.IteratorNext(numberOfResources, iteratorId);
                for (int i = 0; i < rds.Count; i++)
                {
                    if (rds[i] != null)
                    {
                        long gid = rds[i].Id;
                        Discrete discrete = new Discrete(rds[i].Properties.Values.ToList(), ModelCode.ANALOG);
                        CurrentModel[DMSType.Discrete].Add(rds[i].Id, discrete);
                    }
                }
                resourcesLeft = proxy.IteratorResourcesLeft(iteratorId);
            }
        }

    }
}
