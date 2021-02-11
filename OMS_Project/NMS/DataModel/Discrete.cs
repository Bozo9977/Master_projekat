using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class Discrete : Measurement
	{
		public int MaxValue { get; private set; }
		public int MinValue { get; private set; }
		public int NormalValue { get; private set; }

		public Discrete() { }

		public Discrete(Discrete d) : base(d)
		{
			MaxValue = d.MaxValue;
			MinValue = d.MinValue;
			NormalValue = d.NormalValue;
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.DISCRETE_MAXVALUE:
				case ModelCode.DISCRETE_MINVALUE:
				case ModelCode.DISCRETE_NORMALVALUE:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.DISCRETE_MAXVALUE:
					return new Int32Property(ModelCode.DISCRETE_MAXVALUE, MaxValue);

				case ModelCode.DISCRETE_MINVALUE:
					return new Int32Property(ModelCode.DISCRETE_MINVALUE, MinValue);

				case ModelCode.DISCRETE_NORMALVALUE:
					return new Int32Property(ModelCode.DISCRETE_NORMALVALUE, NormalValue);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.DISCRETE_MAXVALUE:
					MaxValue = ((Int32Property)p).Value;
					return true;

				case ModelCode.DISCRETE_MINVALUE:
					MinValue = ((Int32Property)p).Value;
					return true;

				case ModelCode.DISCRETE_NORMALVALUE:
					NormalValue = ((Int32Property)p).Value;
					return true;
			}

			return base.SetProperty(p);
		}

		public override IdentifiedObject Clone()
		{
			return new Discrete(this);
		}
	}
}
