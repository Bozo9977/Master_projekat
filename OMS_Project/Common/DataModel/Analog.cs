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
	public class AnalogDBModel
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
		public float MinValue { get; set; }
		public float MaxValue { get; set; }
		public float NormalValue { get; set; }
	}

	public class Analog : Measurement
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

		public Analog(AnalogDBModel entity)
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

		public override bool SetProperty(Property p, bool force = false)
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

			return base.SetProperty(p, force);
		}

		public override IdentifiedObject Clone()
		{
			return new Analog(this);
		}

		public override object ToDBEntity()
		{
			return new AnalogDBModel() { GID = GID, MRID = MRID, Name = Name, BaseAddress = BaseAddress, Direction = Direction, MeasurementType = MeasurementType, PowerSystemResource = PowerSystemResource, Terminal = Terminal, MinValue = MinValue, MaxValue = MaxValue, NormalValue = NormalValue };
		}

		public override bool Validate(Func<long, IdentifiedObject> entityGetter)
		{
			if(MinValue < 0)
				return false;
			if(MaxValue < 0)
				return false;
			if(NormalValue < 0)
				return false;
			if(MaxValue < MinValue)
				return false;
			if(NormalValue < MinValue)
				return false;
			if(NormalValue > MaxValue)
				return false;

			return base.Validate(entityGetter);
		}
	}
}
