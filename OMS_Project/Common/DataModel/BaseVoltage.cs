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
	public class BaseVoltageDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
		public float NominalVoltage { get; set; }
	}

	public class BaseVoltage : IdentifiedObject
	{
		public float NominalVoltage { get; protected set; }
		public List<long> ConductingEquipment { get; private set; }

		public BaseVoltage()
		{
			ConductingEquipment = new List<long>();
		}

		public BaseVoltage(BaseVoltage b) : base(b)
		{
			NominalVoltage = b.NominalVoltage;
			ConductingEquipment = new List<long>(b.ConductingEquipment);
		}

		public BaseVoltage(BaseVoltageDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			NominalVoltage = entity.NominalVoltage;
			ConductingEquipment = new List<long>();
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

		public override bool SetProperty(Property p, bool force = false)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.BASEVOLTAGE_NOMINALVOLTAGE:
					NominalVoltage = ((FloatProperty)p).Value;
					return true;

				case ModelCode.BASEVOLTAGE_CONDUCTINGEQUIPMENT:
					if(force)
					{
						ConductingEquipment = ((ReferencesProperty)p).Value;
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

		public override object ToDBEntity()
		{
			return new BaseVoltageDBModel() { GID = GID, MRID = MRID, Name = Name, NominalVoltage = NominalVoltage };
		}

		public override void GetEntitiesToValidate(Func<long, IdentifiedObject> entityGetter, HashSet<long> dst)
		{
			foreach(long ce in ConductingEquipment)
				dst.Add(ce);

			base.GetEntitiesToValidate(entityGetter, dst);
		}

		public override bool Validate(Func<long, IdentifiedObject> entityGetter)
		{
			if(NominalVoltage < 0)
				return false;

			return base.Validate(entityGetter);
		}
	}
}
