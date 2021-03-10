using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModel
{
	public abstract class Measurement : IdentifiedObject
	{
		public int BaseAddress { get; protected set; }
		public SignalDirection Direction { get; protected set; }
		public MeasurementType MeasurementType { get; protected set; }
		public long PowerSystemResource { get; protected set; }
		public long Terminal { get; protected set; }

		public Measurement() { }

		public Measurement(Measurement m) : base(m)
		{
			BaseAddress = m.BaseAddress;
			Direction = m.Direction;
			MeasurementType = m.MeasurementType;
			PowerSystemResource = m.PowerSystemResource;
			Terminal = m.Terminal;
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.MEASUREMENT_BASEADDRESS:
				case ModelCode.MEASUREMENT_DIRECTION:
				case ModelCode.MEASUREMENT_MEASUREMENTTYPE:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.MEASUREMENT_BASEADDRESS:
					return new Int32Property(ModelCode.MEASUREMENT_BASEADDRESS, BaseAddress);

				case ModelCode.MEASUREMENT_DIRECTION:
					return new EnumProperty(ModelCode.MEASUREMENT_DIRECTION, (short)Direction);

				case ModelCode.MEASUREMENT_MEASUREMENTTYPE:
					return new EnumProperty(ModelCode.MEASUREMENT_MEASUREMENTTYPE, (short)MeasurementType);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p, bool force = false)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.MEASUREMENT_BASEADDRESS:
					BaseAddress = ((Int32Property)p).Value;
					return true;

				case ModelCode.MEASUREMENT_DIRECTION:
					Direction = (SignalDirection)((EnumProperty)p).Value;
					return true;

				case ModelCode.MEASUREMENT_MEASUREMENTTYPE:
					MeasurementType = (MeasurementType)((EnumProperty)p).Value;
					return true;
			}

			return base.SetProperty(p, force);
		}

		public override void GetSourceReferences(Dictionary<ModelCode, long> dst)
		{
			dst[ModelCode.MEASUREMENT_POWERSYSTEMRESOURCE] = PowerSystemResource;
			dst[ModelCode.MEASUREMENT_TERMINAL] = Terminal;
			base.GetSourceReferences(dst);
		}


        // VALIDATION
        public override bool Validate(Func<long, IdentifiedObject> entityGetter)
        {
			if (BaseAddress < 0)
				return false;

            return base.Validate(entityGetter);
        }
    }
}
