using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class DistributionGenerator : ConductingEquipment
	{
		public float RatedCosPhi { get; private set; }
		public float RatedPower { get; private set; }
		public float RatedVoltage { get; private set; }

		public DistributionGenerator() { }

		public DistributionGenerator(DistributionGenerator d) : base(d)
		{
			RatedCosPhi = d.RatedCosPhi;
			RatedPower = d.RatedPower;
			RatedVoltage = d.RatedVoltage;
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.DISTRIBUTIONGENERATOR_RATEDCOSPHI:
				case ModelCode.DISTRIBUTIONGENERATOR_RATEDPOWER:
				case ModelCode.DISTRIBUTIONGENERATOR_RATEDVOLTAGE:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.DISTRIBUTIONGENERATOR_RATEDCOSPHI:
					return new FloatProperty(ModelCode.DISTRIBUTIONGENERATOR_RATEDCOSPHI, RatedCosPhi);
				case ModelCode.DISTRIBUTIONGENERATOR_RATEDPOWER:
					return new FloatProperty(ModelCode.DISTRIBUTIONGENERATOR_RATEDPOWER, RatedPower);
				case ModelCode.DISTRIBUTIONGENERATOR_RATEDVOLTAGE:
					return new FloatProperty(ModelCode.DISTRIBUTIONGENERATOR_RATEDVOLTAGE, RatedVoltage);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.DISTRIBUTIONGENERATOR_RATEDCOSPHI:
					RatedCosPhi = ((FloatProperty)p).Value;
					return true;
				case ModelCode.DISTRIBUTIONGENERATOR_RATEDPOWER:
					RatedPower = ((FloatProperty)p).Value;
					return true;
				case ModelCode.DISTRIBUTIONGENERATOR_RATEDVOLTAGE:
					RatedVoltage = ((FloatProperty)p).Value;
					return true;
			}

			return base.SetProperty(p);
		}

		public override IdentifiedObject Clone()
		{
			return new DistributionGenerator(this);
		}
	}
}
