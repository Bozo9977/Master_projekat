using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	abstract class PowerSystemResource : IdentifiedObject
	{
		public List<long> Measurements { get; private set; }

		public PowerSystemResource() { }

		public PowerSystemResource(PowerSystemResource p) : base(p)
		{
			Measurements = new List<long>(p.Measurements);
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.POWERSYSTEMRESOURCE_MEASUREMENTS:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.POWERSYSTEMRESOURCE_MEASUREMENTS:
					return new ReferencesProperty(ModelCode.POWERSYSTEMRESOURCE_MEASUREMENTS, Measurements);
			}

			return base.GetProperty(p);
		}

		public override bool AddTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.MEASUREMENT_POWERSYSTEMRESOURCE:
					if(Measurements.Contains(sourceGID))
						return false;

					Measurements.Add(sourceGID);
					return true;
			}

			return base.AddTargetReference(sourceProperty, sourceGID);
		}

		public override bool RemoveTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.MEASUREMENT_POWERSYSTEMRESOURCE:
					return Measurements.Remove(sourceGID);
			}

			return base.RemoveTargetReference(sourceProperty, sourceGID);
		}

		public override void GetTargetReferences(Dictionary<ModelCode, List<long>> dst)
		{
			dst[ModelCode.POWERSYSTEMRESOURCE_MEASUREMENTS] = new List<long>(Measurements);
			base.GetTargetReferences(dst);
		}

		public override bool IsReferenced()
		{
			return Measurements.Count > 0 || base.IsReferenced();
		}
	}
}
