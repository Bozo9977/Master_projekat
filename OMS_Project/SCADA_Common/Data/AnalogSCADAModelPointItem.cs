using Common.GDA;
using SCADA_Common.DB_Model;
using System.Collections.Generic;

namespace SCADA_Common.Data
{
    public class AnalogSCADAModelPointItem : SCADAModelPointItem, IAnalogSCADAModelPointItem
    {
        public float NormalValue { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }

        public AnalogSCADAModelPointItem(List<Property> props, ModelCode type)
            : base(props, type)
        {
            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.ANALOG_MAXVALUE:
                        MaxValue = ((FloatProperty)item).Value;
                        break;

                    case ModelCode.ANALOG_MINVALUE:
                        MinValue = ((FloatProperty)item).Value;
                        break;

                    case ModelCode.ANALOG_NORMALVALUE:
                        NormalValue = ((FloatProperty)item).Value;
                        break;
                    default:
                        break;
                }
            }

            Initialized = true;
        }

        public AnalogSCADAModelPointItem(PointItemDB point) : base(point)
        {
            NormalValue = ((AnalogPointItemDB)point).NormalValue;
            MinValue = ((AnalogPointItemDB)point).MinValue;
            MaxValue = ((AnalogPointItemDB)point).MaxValue;
        }

        public AnalogPointItemDB ToDBEntity()
        {
            return new AnalogPointItemDB
            {
                Gid = base.Gid,
                Address = base.Address,
                Name = base.Name,
                RegisterType = base.RegisterType,
                Alarm = false,
                MinValue = this.MinValue,
                MaxValue = this.MaxValue,
                NormalValue = this.NormalValue
            };
        }
    }
}
