﻿using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModel
{
	public abstract class Switch : ConductingEquipment
	{
		public bool NormalOpen { get; protected set; }
		public List<long> SwitchingSteps { get; private set; }

		public Switch()
		{
			SwitchingSteps = new List<long>();
		}

		public Switch(Switch s) : base(s)
		{
			NormalOpen = s.NormalOpen;
			SwitchingSteps = new List<long>(s.SwitchingSteps);
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.SWITCH_NORMALOPEN:
				case ModelCode.SWITCH_SWITCHINGSTEPS:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.SWITCH_NORMALOPEN:
					return new BoolProperty(ModelCode.SWITCH_NORMALOPEN, NormalOpen);

				case ModelCode.SWITCH_SWITCHINGSTEPS:
					return new ReferencesProperty(ModelCode.SWITCH_SWITCHINGSTEPS, SwitchingSteps);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p, bool force = false)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.SWITCH_NORMALOPEN:
					NormalOpen = ((BoolProperty)p).Value;
					return true;

				case ModelCode.SWITCH_SWITCHINGSTEPS:
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
				case ModelCode.SWITCHINGSTEP_SWITCH:
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
				case ModelCode.SWITCHINGSTEP_SWITCH:
					return SwitchingSteps.Remove(sourceGID);
			}

			return base.RemoveTargetReference(sourceProperty, sourceGID);
		}

		public override void GetTargetReferences(Dictionary<ModelCode, List<long>> dst)
		{
			dst[ModelCode.SWITCH_SWITCHINGSTEPS] = new List<long>(SwitchingSteps);
			base.GetTargetReferences(dst);
		}

		public override bool IsReferenced()
		{
			return SwitchingSteps.Count > 0 || base.IsReferenced();
		}
	}
}
