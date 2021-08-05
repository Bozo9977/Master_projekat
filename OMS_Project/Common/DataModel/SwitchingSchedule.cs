using Common.GDA;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.DataModel
{
	public class SwitchingScheduleDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
	}

	public class SwitchingSchedule : Document
	{
		public List<long> SwitchingSteps { get; private set; }

		public SwitchingSchedule()
		{
			SwitchingSteps = new List<long>();
		}

		public SwitchingSchedule(SwitchingSchedule ss) : base(ss)
		{
			SwitchingSteps = new List<long>(ss.SwitchingSteps);
		}

		public SwitchingSchedule(SwitchingScheduleDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			SwitchingSteps = new List<long>();
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.SWITCHINGSCHEDULE_SWITCHINGSTEPS:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.SWITCHINGSCHEDULE_SWITCHINGSTEPS:
					return new ReferencesProperty(ModelCode.SWITCHINGSCHEDULE_SWITCHINGSTEPS, SwitchingSteps);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p, bool force = false)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.SWITCHINGSCHEDULE_SWITCHINGSTEPS:
					if(force)
					{
						SwitchingSteps = ((ReferencesProperty)p).Value;
						return true;
					}
					return false;
			}

			return base.SetProperty(p, force);
		}

		public override bool AddTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.SWITCHINGSTEP_SWITCHINGSCHEDULE:
					if(SwitchingSteps.Contains(sourceGID))
						return false;

					SwitchingSteps.Add(sourceGID);
					return true;
			}

			return base.AddTargetReference(sourceProperty, sourceGID);
		}

		public override bool RemoveTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.SWITCHINGSTEP_SWITCHINGSCHEDULE:
					return SwitchingSteps.Remove(sourceGID);
			}

			return base.RemoveTargetReference(sourceProperty, sourceGID);
		}

		public override void GetTargetReferences(Dictionary<ModelCode, List<long>> dst)
		{
			dst[ModelCode.SWITCHINGSCHEDULE_SWITCHINGSTEPS] = new List<long>(SwitchingSteps);
			base.GetTargetReferences(dst);
		}

		public override bool IsReferenced()
		{
			return SwitchingSteps.Count > 0 || base.IsReferenced();
		}

		public override IdentifiedObject Clone()
		{
			return new SwitchingSchedule(this);
		}

		public override object ToDBEntity()
		{
			return new SwitchingScheduleDBModel() { GID = GID, MRID = MRID, Name = Name };
		}

		public override void GetEntitiesToValidate(Func<long, IdentifiedObject> entityGetter, HashSet<long> dst)
		{
			foreach(long ss in SwitchingSteps)
				dst.Add(ss);

			base.GetEntitiesToValidate(entityGetter, dst);
		}
	}
}
