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
	public class TerminalDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
		public long ConnectivityNode { get; set; }
		public long ConductingEquipment { get; set; }
	}

	public class Terminal : IdentifiedObject
	{
		public long ConnectivityNode { get; protected set; }
		public long ConductingEquipment { get; protected set; }
		public List<long> Measurements { get; private set; }

		public Terminal()
		{
			Measurements = new List<long>();
		}

		public Terminal(Terminal t) : base(t)
		{
			ConnectivityNode = t.ConnectivityNode;
			ConductingEquipment = t.ConductingEquipment;
			Measurements = new List<long>(t.Measurements);
		}

		public Terminal(TerminalDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			ConnectivityNode = entity.ConnectivityNode;
			ConductingEquipment = entity.ConductingEquipment;
			Measurements = new List<long>();
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.TERMINAL_CONNECTIVITYNODE:
				case ModelCode.TERMINAL_CONDUCTINGEQUIPMENT:
				case ModelCode.TERMINAL_MEASUREMENTS:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.TERMINAL_CONNECTIVITYNODE:
					return new ReferenceProperty(ModelCode.TERMINAL_CONNECTIVITYNODE, ConnectivityNode);
				case ModelCode.TERMINAL_CONDUCTINGEQUIPMENT:
					return new ReferenceProperty(ModelCode.TERMINAL_CONDUCTINGEQUIPMENT, ConductingEquipment);
				case ModelCode.TERMINAL_MEASUREMENTS:
					return new ReferencesProperty(ModelCode.TERMINAL_MEASUREMENTS, Measurements);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p, bool force = false)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.TERMINAL_CONNECTIVITYNODE:
					ConnectivityNode = ((ReferenceProperty)p).Value;
					return true;
				case ModelCode.TERMINAL_CONDUCTINGEQUIPMENT:
					ConductingEquipment = ((ReferenceProperty)p).Value;
					return true;

				case ModelCode.TERMINAL_MEASUREMENTS:
					if(force)
					{
						Measurements = ((ReferencesProperty)p).Value;
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
				case ModelCode.MEASUREMENT_TERMINAL:
					if(Measurements.Contains(sourceGID))
						return false;

					Measurements.Add(sourceGID);
					return true;
			}

			return base.AddTargetReference(sourceProperty, sourceGID);
		}

		public override bool RemoveTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.MEASUREMENT_TERMINAL:
					return Measurements.Remove(sourceGID);
			}

			return base.RemoveTargetReference(sourceProperty, sourceGID);
		}

		public override void GetTargetReferences(Dictionary<ModelCode, List<long>> dst)
		{
			dst[ModelCode.TERMINAL_MEASUREMENTS] = new List<long>(Measurements);
			base.GetTargetReferences(dst);
		}

		public override bool IsReferenced()
		{
			return Measurements.Count > 0 || base.IsReferenced();
		}

		public override void GetSourceReferences(Dictionary<ModelCode, long> dst)
		{
			dst[ModelCode.TERMINAL_CONNECTIVITYNODE] = ConnectivityNode;
			dst[ModelCode.TERMINAL_CONDUCTINGEQUIPMENT] = ConductingEquipment;
			base.GetSourceReferences(dst);
		}

		public override IdentifiedObject Clone()
		{
			return new Terminal(this);
		}

		public override object ToDBEntity()
		{
			return new TerminalDBModel() { GID = GID, MRID = MRID, Name = Name, ConnectivityNode = ConnectivityNode, ConductingEquipment = ConductingEquipment };
		}


		// VALIDATION
        public override void GetEntitiesToValidate(Func<long, IdentifiedObject> entityGetter, HashSet<long> dst)
        {
			foreach (var k in Measurements)
				dst.Add(k);

            base.GetEntitiesToValidate(entityGetter, dst);
        }
    }
}
