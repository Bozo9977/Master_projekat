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
	public class ACLineSegmentDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
		public long BaseVoltage { get; set; }
		public float RatedCurrent { get; set; }
	}

	public class ACLineSegment : Conductor
	{
		public float RatedCurrent { get; protected set; }

		public ACLineSegment() { }

		public ACLineSegment(ACLineSegment a) : base(a)
		{
			RatedCurrent = a.RatedCurrent;
		}

		public ACLineSegment(ACLineSegmentDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			BaseVoltage = entity.BaseVoltage;
			RatedCurrent = entity.RatedCurrent;
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

		public override bool SetProperty(Property p, bool force = false)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.ACLINESEGMENT_RATEDCURRENT:
					RatedCurrent = ((FloatProperty)p).Value;
					return true;
			}

			return base.SetProperty(p, force);
		}

		public override IdentifiedObject Clone()
		{
			return new ACLineSegment(this);
		}

		public override object ToDBEntity()
		{
			return new ACLineSegmentDBModel() { GID = GID, MRID = MRID, Name = Name, BaseVoltage = BaseVoltage, RatedCurrent = RatedCurrent };
		}


		//validation
        public override void GetEntitiesToValidate(Func<long, IdentifiedObject> entityGetter, HashSet<long> dst)
        {
            base.GetEntitiesToValidate(entityGetter, dst);
        }

        public override bool Validate(Func<long, IdentifiedObject> entityGetter)
        {
			if (RatedCurrent < 0)
				return false;

            return base.Validate(entityGetter);
        }
    }
}
