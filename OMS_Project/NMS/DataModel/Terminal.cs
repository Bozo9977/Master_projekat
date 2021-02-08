using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class Terminal : IdentifiedObject
	{
		public long ConnectivityNode { get; private set; }
		public long ConductingEquipment { get; private set; }
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

		public override bool SetProperty(Property p)
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
			}

			return base.SetProperty(p);
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
	}
}
