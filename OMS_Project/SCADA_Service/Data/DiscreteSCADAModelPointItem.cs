using Common.GDA;
using SCADA_Common.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Service.Data
{
    public class DiscreteSCADAModelPointItem : SCADAModelPointItem, IDiscreteSCADAModelPointItem
    {
        private ushort currentValue;

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

        public ushort MinValue { get; set; }
        public ushort MaxValue { get; set; }
        public ushort NormalValue { get; set; }
        public ushort CurrentValue
        {
            get { return currentValue; }
            set
            {
                currentValue = value;
            }
        }
        public ushort AbnormalValue { get; set; }
    }
}
