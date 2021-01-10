using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class BaseVoltage : IdentifiedObject
	{
		public float NominalVoltage { get; private set; }
		public List<long> ConductingEquipment { get; private set; }

		public BaseVoltage() { }

		public BaseVoltage(BaseVoltage b) : base(b)
		{
			NominalVoltage = b.NominalVoltage;
			ConductingEquipment = new List<long>(b.ConductingEquipment);
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.BASEVOLTAGE_NOMINALVOLTAGE:
				case ModelCode.BASEVOLTAGE_CONDUCTINGEQUIPMENT:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.BASEVOLTAGE_NOMINALVOLTAGE:
					return new FloatProperty(ModelCode.BASEVOLTAGE_NOMINALVOLTAGE, NominalVoltage);

				case ModelCode.BASEVOLTAGE_CONDUCTINGEQUIPMENT:
					return new ReferencesProperty(ModelCode.BASEVOLTAGE_CONDUCTINGEQUIPMENT, ConductingEquipment);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.BASEVOLTAGE_NOMINALVOLTAGE:
					NominalVoltage = ((FloatProperty)p).Value;
					return true;
			}

			return base.SetProperty(p);
		}

		public override bool AddTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE:
					if(ConductingEquipment.Contains(sourceGID))
						return false;

					ConductingEquipment.Add(sourceGID);
					return true;
			}

			return base.AddTargetReference(sourceProperty, sourceGID);
		}

		public override bool RemoveTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.CONDUCTINGEQUIPMENT_BASEVOLTAGE:
					return ConductingEquipment.Remove(sourceGID);
			}

			return base.RemoveTargetReference(sourceProperty, sourceGID);
		}

		public override void GetTargetReferences(Dictionary<ModelCode, List<long>> dst)
		{
			dst[ModelCode.BASEVOLTAGE_CONDUCTINGEQUIPMENT] = new List<long>(ConductingEquipment);
			base.GetTargetReferences(dst);
		}

		public override bool IsReferenced()
		{
			return ConductingEquipment.Count > 0 || base.IsReferenced();
		}

		public override IdentifiedObject Clone()
		{
			return new BaseVoltage(this);
		}
	}
}
