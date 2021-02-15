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
	public class DiscreteDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
		public int BaseAddress { get; set; }
		public SignalDirection Direction { get; set; }
		public MeasurementType MeasurementType { get; set; }
		public long PowerSystemResource { get; set; }
		public long Terminal { get; set; }
		public int MinValue { get; set; }
		public int MaxValue { get; set; }
		public int NormalValue { get; set; }
	}

	public class Discrete : Measurement
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

		public Discrete(DiscreteDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			BaseAddress = entity.BaseAddress;
			Direction = entity.Direction;
			MeasurementType = entity.MeasurementType;
			PowerSystemResource = entity.PowerSystemResource;
			Terminal = entity.Terminal;
			MinValue = entity.MinValue;
			MaxValue = entity.MaxValue;
			NormalValue = entity.NormalValue;
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

		public override object ToDBEntity()
		{
			return new DiscreteDBModel() { GID = GID, MRID = MRID, Name = Name, BaseAddress = BaseAddress, Direction = Direction, MeasurementType = MeasurementType, PowerSystemResource = PowerSystemResource, Terminal = Terminal, MinValue = MinValue, MaxValue = MaxValue, NormalValue = NormalValue };
		}
	}
}
