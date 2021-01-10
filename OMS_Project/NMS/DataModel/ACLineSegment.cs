using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class ACLineSegment : Conductor
	{
		public float RatedCurrent { get; private set; }

		public ACLineSegment() { }

		public ACLineSegment(ACLineSegment a) : base(a)
		{
			RatedCurrent = a.RatedCurrent;
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.ACLINESEGMENT_RATEDCURRENT:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.ACLINESEGMENT_RATEDCURRENT:
					return new FloatProperty(ModelCode.ACLINESEGMENT_RATEDCURRENT, RatedCurrent);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.ACLINESEGMENT_RATEDCURRENT:
					RatedCurrent = ((FloatProperty)p).Value;
					return true;
			}

			return base.SetProperty(p);
		}

		public override IdentifiedObject Clone()
		{
			return new ACLineSegment(this);
		}
	}
}
