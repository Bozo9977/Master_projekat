using Common.GDA;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Common.DataModel
{
	public class SwitchingStepDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
		public long SwitchingSchedule { get; set; }
		public long Switch { get; set; }
		public bool Open { get; set; }
		public int Index { get; set; }
	}

	public class SwitchingStep : IdentifiedObject
	{
		public long SwitchingSchedule { get; protected set; }
		public long Switch { get; protected set; }
		public bool Open { get; protected set; }
		public int Index { get; protected set; }

		public SwitchingStep()
		{ }

		public SwitchingStep(SwitchingStep ss) : base(ss)
		{
			SwitchingSchedule = ss.SwitchingSchedule;
			Switch = ss.Switch;
			Open = ss.Open;
			Index = ss.Index;
		}

		public SwitchingStep(SwitchingStepDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			SwitchingSchedule = entity.SwitchingSchedule;
			Switch = entity.Switch;
			Open = entity.Open;
			Index = entity.Index;
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.SWITCHINGSTEP_SWITCHINGSCHEDULE:
				case ModelCode.SWITCHINGSTEP_SWITCH:
				case ModelCode.SWITCHINGSTEP_OPEN:
				case ModelCode.SWITCHINGSTEP_INDEX:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.SWITCHINGSTEP_SWITCHINGSCHEDULE:
					return new ReferenceProperty(ModelCode.SWITCHINGSTEP_SWITCHINGSCHEDULE, SwitchingSchedule);

				case ModelCode.SWITCHINGSTEP_SWITCH:
					return new ReferenceProperty(ModelCode.SWITCHINGSTEP_SWITCH, Switch);

				case ModelCode.SWITCHINGSTEP_OPEN:
					return new BoolProperty(ModelCode.SWITCHINGSTEP_OPEN, Open);

				case ModelCode.SWITCHINGSTEP_INDEX:
					return new Int32Property(ModelCode.SWITCHINGSTEP_INDEX, Index);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p, bool force = false)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.SWITCHINGSTEP_SWITCHINGSCHEDULE:
					SwitchingSchedule = ((ReferenceProperty)p).Value;
					return true;

				case ModelCode.SWITCHINGSTEP_SWITCH:
					Switch = ((ReferenceProperty)p).Value;
					return true;

				case ModelCode.SWITCHINGSTEP_OPEN:
					Open = ((BoolProperty)p).Value;
					return true;

				case ModelCode.SWITCHINGSTEP_INDEX:
					Index = ((Int32Property)p).Value;
					return true;
			}

			return base.SetProperty(p, force);
		}

		public override void GetSourceReferences(Dictionary<ModelCode, long> dst)
		{
			dst[ModelCode.SWITCHINGSTEP_SWITCHINGSCHEDULE] = SwitchingSchedule;
			dst[ModelCode.SWITCHINGSTEP_SWITCH] = Switch;
			dst[ModelCode.SWITCHINGSTEP_INDEX] = Index;
			base.GetSourceReferences(dst);
		}

		public override IdentifiedObject Clone()
		{
			return new SwitchingStep(this);
		}

		public override object ToDBEntity()
		{
			return new SwitchingStepDBModel() { GID = GID, MRID = MRID, Name = Name, SwitchingSchedule = SwitchingSchedule, Switch = Switch, Open = Open, Index = Index };
		}

		public override void GetEntitiesToValidate(Func<long, IdentifiedObject> entityGetter, HashSet<long> dst)
		{
			dst.Add(SwitchingSchedule);

			base.GetEntitiesToValidate(entityGetter, dst);
		}
	}
}
