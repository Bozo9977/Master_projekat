using Common.GDA;
using SCADA_Common;
using SCADA_Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Service.Data
{
    public abstract class SCADAModelPointItem : ISCADAModelPointItem
    {
        public long Gid { get; set; }
        public ushort Address { get; set; }
        public string Name { get; set; }
        public PointType RegisterType { get; set; }
        public AlarmType Alarm { get; set; }
        public bool Initialized { get; protected set; }

        protected SCADAModelPointItem(List<Property> props, ModelCode type)
        {
            Alarm = AlarmType.NO_ALARM;

            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.IDENTIFIEDOBJECT_GID:
                        Gid = ((Int64Property)item).Value;
                        break;

                    case ModelCode.IDENTIFIEDOBJECT_NAME:
                        Name = ((StringProperty)item).Value;
                        break;

                    case ModelCode.MEASUREMENT_BASEADDRESS:
                        Address = ushort.Parse(((Int32Property)item).Value.ToString());
                        break;

                    case ModelCode.MEASUREMENT_DIRECTION:
                        if (type == ModelCode.ANALOG)
                        {
                            RegisterType = (((EnumProperty)item).Value != 0) ? PointType.ANALOG_INPUT : PointType.ANALOG_OUTPUT;
                        }
                        else if (type == ModelCode.DISCRETE)
                        {
                            RegisterType = (((EnumProperty)item).Value != 0) ? PointType.DIGITAL_INPUT : PointType.DIGITAL_OUTPUT;
                        }
                        else
                        {
                            string message = "SCADAModelPointItem constructor => ModelCode type is neither ANALOG nor DISCRETE.";
                            throw new ArgumentException(message);
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
