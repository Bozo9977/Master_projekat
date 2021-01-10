using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	abstract class Switch : ConductingEquipment
	{
		public bool NormalOpen { get; private set; }

		public Switch() { }

		public Switch(Switch s) : base(s)
		{
			NormalOpen = s.NormalOpen;
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.SWITCH_NORMALOPEN:
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
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.SWITCH_NORMALOPEN:
					NormalOpen = ((BoolProperty)p).Value;
					return true;
			}

			return base.SetProperty(p);
		}
	}
}
