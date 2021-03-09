using Common.GDA;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModel
{
	public class DistributionGeneratorDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
		public long BaseVoltage { get; set; }
		public float RatedCosPhi { get; set; }
		public float RatedPower { get; set; }
		public float RatedVoltage { get; set; }
	}

	public class DistributionGenerator : ConductingEquipment
	{
		public float RatedCosPhi { get; protected set; }
		public float RatedPower { get; protected set; }
		public float RatedVoltage { get; protected set; }

		public DistributionGenerator() { }

		public DistributionGenerator(DistributionGenerator d) : base(d)
		{
			RatedCosPhi = d.RatedCosPhi;
			RatedPower = d.RatedPower;
			RatedVoltage = d.RatedVoltage;
		}

		public DistributionGenerator(DistributionGeneratorDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			BaseVoltage = entity.BaseVoltage;
			RatedCosPhi = entity.RatedCosPhi;
			RatedPower = entity.RatedPower;
			RatedVoltage = entity.RatedVoltage;
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

		public override bool SetProperty(Property p, bool force = false)
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

			return base.SetProperty(p, force);
		}

		public override IdentifiedObject Clone()
		{
			return new DistributionGenerator(this);
		}

		public override object ToDBEntity()
		{
			return new DistributionGeneratorDBModel() { GID = GID, MRID = MRID, Name = Name, BaseVoltage = BaseVoltage, RatedCosPhi = RatedCosPhi, RatedPower = RatedPower, RatedVoltage = RatedVoltage };
		}
	}
}
