using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModel
{
	public abstract class ConductingEquipment : Equipment
	{
		public long BaseVoltage { get; protected set; }
		public List<long> Terminals { get; private set; }

		public ConductingEquipment()
		{
			Terminals = new List<long>();
		}

		public ConductingEquipment(ConductingEquipment c) : base(c)
		{
			BaseVoltage = c.BaseVoltage;
			Terminals = new List<long>(c.Terminals);
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE:
				case ModelCode.CONDUCTINGEQUIPMENT_TERMINALS:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE:
					return new ReferenceProperty(ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE, BaseVoltage);
				case ModelCode.CONDUCTINGEQUIPMENT_TERMINALS:
					return new ReferencesProperty(ModelCode.CONDUCTINGEQUIPMENT_TERMINALS, Terminals);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE:
					BaseVoltage = ((ReferenceProperty)p).Value;
					return true;
			}

			return base.SetProperty(p);
		}

		public override bool AddTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.TERMINAL_CONDUCTINGEQUIPMENT:
					if(Terminals.Contains(sourceGID))
						return false;

					Terminals.Add(sourceGID);
					return true;
			}

			return base.AddTargetReference(sourceProperty, sourceGID);
		}

		public override bool RemoveTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.TERMINAL_CONDUCTINGEQUIPMENT:
					return Terminals.Remove(sourceGID);
			}

			return base.RemoveTargetReference(sourceProperty, sourceGID);
		}

		public override void GetTargetReferences(Dictionary<ModelCode, List<long>> dst)
		{
			dst[ModelCode.CONDUCTINGEQUIPMENT_TERMINALS] = new List<long>(Terminals);
			base.GetTargetReferences(dst);
		}

		public override bool IsReferenced()
		{
			return Terminals.Count > 0 || base.IsReferenced();
		}

		public override void GetSourceReferences(Dictionary<ModelCode, long> dst)
		{
			dst[ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE] = BaseVoltage;
			base.GetSourceReferences(dst);
		}
	}
}
