using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class Analog : Measurement
	{
		public float MinValue { get; private set; }
		public float MaxValue { get; private set; }
		public float NormalValue { get; private set; }

		public Analog() { }

		public Analog(Analog a) : base(a)
		{
			MinValue = a.MinValue;
			MaxValue = a.MaxValue;
			NormalValue = a.NormalValue;
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.ANALOG_MAXVALUE:
				case ModelCode.ANALOG_MINVALUE:
				case ModelCode.ANALOG_NORMALVALUE:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.ANALOG_MAXVALUE:
					return new FloatProperty(ModelCode.ANALOG_MAXVALUE, MaxValue);

				case ModelCode.ANALOG_MINVALUE:
					return new FloatProperty(ModelCode.ANALOG_MINVALUE, MinValue);

				case ModelCode.ANALOG_NORMALVALUE:
					return new FloatProperty(ModelCode.ANALOG_NORMALVALUE, NormalValue);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.ANALOG_MAXVALUE:
					MaxValue = ((FloatProperty)p).Value;
					return true;

				case ModelCode.ANALOG_MINVALUE:
					MinValue = ((FloatProperty)p).Value;
					return true;

				case ModelCode.ANALOG_NORMALVALUE:
					NormalValue = ((FloatProperty)p).Value;
					return true;
			}

			return base.SetProperty(p);
		}

		public override IdentifiedObject Clone()
		{
			return new Analog(this);
		}
	}
}
