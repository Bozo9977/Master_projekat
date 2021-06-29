using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModel
{
	public abstract class Conductor : ConductingEquipment
	{
		public float Length { get; protected set; }

		public Conductor() { }

		public Conductor(Conductor c) : base(c)
		{
			Length = c.Length;
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.CONDUCTOR_LENGTH:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.CONDUCTOR_LENGTH:
					return new FloatProperty(ModelCode.CONDUCTOR_LENGTH, Length);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p, bool force = false)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.CONDUCTOR_LENGTH:
					Length = ((FloatProperty)p).Value;
					return true;
			}

			return base.SetProperty(p, force);
		}
	}
}
