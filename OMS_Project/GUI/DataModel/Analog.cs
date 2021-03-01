using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.DataModel
{
    public class Analog :  Measurement
    {
		public float MinValue { get; protected set; }
		public float MaxValue { get; protected set; }
		public float NormalValue { get; protected set; }

		public Analog() { }

		public Analog(Analog a) : base(a)
		{
			MinValue = a.MinValue;
			MaxValue = a.MaxValue;
			NormalValue = a.NormalValue;
		}

		public Analog(List<Property> props, ModelCode code) : base(props, code)
        {
			foreach (var prop in props)
			{
                switch (prop.Id)
                {
                    case ModelCode.ANALOG_MAXVALUE:
                        MaxValue = ((FloatProperty)prop).Value;
                        break;

                    case ModelCode.ANALOG_MINVALUE:
                        MinValue = ((FloatProperty)prop).Value;
                        break;

                    case ModelCode.ANALOG_NORMALVALUE:
                        NormalValue = ((FloatProperty)prop).Value;
                        break;

                    /*case ModelCode.ANALOG_SCALINGFACTOR:
                        ScalingFactor = item.AsFloat();
                        break;

                    case ModelCode.ANALOG_DEVIATION:
                        Deviation = item.AsFloat();
                        break;*/

                    default:
                        break;
                }
            }
		}
	}
}
