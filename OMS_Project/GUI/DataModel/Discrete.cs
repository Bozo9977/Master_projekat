using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.DataModel
{
    public class Discrete :  Measurement
    {
		public int MaxValue { get; protected set; }
		public int MinValue { get; protected set; }
		public int NormalValue { get; protected set; }

		public Discrete() { }

		public Discrete(Discrete d) : base(d)
		{
			MaxValue = d.MaxValue;
			MinValue = d.MinValue;
			NormalValue = d.NormalValue;
		}

        public Discrete(List<Property> props, ModelCode code) : base(props, code)
        {
            foreach (var prop in props)
            {
                switch (prop.Id)
                {
                    case ModelCode.DISCRETE_MAXVALUE:
                        MaxValue = ((Int32Property)prop).Value;
                        break;

                    case ModelCode.DISCRETE_MINVALUE:
                        MinValue = ((Int32Property)prop).Value;
                        break;

                    case ModelCode.DISCRETE_NORMALVALUE:
                        NormalValue = ((Int32Property)prop).Value;
                        break;

                    default:
                        break;
                }
            }
        }
    }
}
