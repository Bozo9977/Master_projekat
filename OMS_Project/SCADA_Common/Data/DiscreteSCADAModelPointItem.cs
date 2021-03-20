using Common.GDA;
using SCADA_Common.DB_Model;
using System.Collections.Generic;

namespace SCADA_Common.Data
{
    public class DiscreteSCADAModelPointItem : SCADAModelPointItem, IDiscreteSCADAModelPointItem
    {

        public ushort MinValue { get; set; }
        public ushort MaxValue { get; set; }
        public ushort NormalValue { get; set; }

        public DiscreteSCADAModelPointItem(List<Property> props, ModelCode type)
            : base(props, type)
        {
            foreach (var item in props)
            {
                switch (item.Id)
                {
                    case ModelCode.DISCRETE_MAXVALUE:
                        MaxValue = (ushort)((Int32Property)item).Value;
                        break;

                    case ModelCode.DISCRETE_MINVALUE:
                        MinValue = (ushort)((Int32Property)item).Value;
                        break;

                    case ModelCode.DISCRETE_NORMALVALUE:
                        NormalValue = (ushort)((Int32Property)item).Value;
                        break;

                    default:
                        break;
                }
            }

            Initialized = true;
        }

        public DiscreteSCADAModelPointItem(PointItemDB dbPoint) : base(dbPoint)
        {
            MinValue = (ushort)((DiscretePointItemDB)dbPoint).MinValue;
            MaxValue = (ushort)((DiscretePointItemDB)dbPoint).MaxValue;
            NormalValue = (ushort)((DiscretePointItemDB)dbPoint).CurrentValue;
        }


        public DiscretePointItemDB ToDBEntity()
        {
            return new DiscretePointItemDB
            {
                Gid = base.Gid,
                Address = base.Address,
                Name = base.Name,
                RegisterType = base.RegisterType,
                Alarm = false,
                MinValue = (short)this.MinValue,
                MaxValue = (short)this.MaxValue,
                CurrentValue = (short)this.NormalValue
            };
        }
    }
}
