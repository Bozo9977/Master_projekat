using Common.GDA;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	public class EnergyConsumerDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
		public long BaseVoltage { get; set; }
		public float PFixed { get; set; }
		public float QFixed { get; set; }
		public ConsumerClass ConsumerClass { get; set; }
	}

	public class EnergyConsumer : ConductingEquipment
	{
		public float PFixed { get; protected set; }
		public float QFixed { get; protected set; }
		public ConsumerClass ConsumerClass { get; protected set; }

		public EnergyConsumer() { }

		public EnergyConsumer(EnergyConsumer e) : base(e)
		{
			PFixed = e.PFixed;
			QFixed = e.QFixed;
			ConsumerClass = e.ConsumerClass;
		}

		public EnergyConsumer(EnergyConsumerDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			BaseVoltage = entity.BaseVoltage;
			PFixed = entity.PFixed;
			QFixed = entity.QFixed;
			ConsumerClass = entity.ConsumerClass;
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.ENERGYCONSUMER_PFIXED:
				case ModelCode.ENERGYCONSUMER_QFIXED:
				case ModelCode.ENERGYCONSUMER_CONSUMERCLASS:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.ENERGYCONSUMER_PFIXED:
					return new FloatProperty(ModelCode.ENERGYCONSUMER_PFIXED, PFixed);
				case ModelCode.ENERGYCONSUMER_QFIXED:
					return new FloatProperty(ModelCode.ENERGYCONSUMER_QFIXED, QFixed);
				case ModelCode.ENERGYCONSUMER_CONSUMERCLASS:
					return new EnumProperty(ModelCode.ENERGYCONSUMER_CONSUMERCLASS, (short)ConsumerClass);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.ENERGYCONSUMER_PFIXED:
					PFixed = ((FloatProperty)p).Value;
					return true;
				case ModelCode.ENERGYCONSUMER_QFIXED:
					QFixed = ((FloatProperty)p).Value;
					return true;
				case ModelCode.ENERGYCONSUMER_CONSUMERCLASS:
					ConsumerClass = (ConsumerClass)((EnumProperty)p).Value;
					return true;
			}

			return base.SetProperty(p);
		}

		public override IdentifiedObject Clone()
		{
			return new EnergyConsumer(this);
		}

		public override object ToDBEntity()
		{
			return new EnergyConsumerDBModel() { GID = GID, MRID = MRID, Name = Name, BaseVoltage = BaseVoltage, PFixed = PFixed, QFixed = QFixed, ConsumerClass = ConsumerClass };
		}
	}
}
