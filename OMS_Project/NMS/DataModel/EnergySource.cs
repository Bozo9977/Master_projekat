using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class EnergySource : ConductingEquipment
	{
		public EnergySource() { }

		public EnergySource(EnergySource e) : base(e)
		{ }

		public override IdentifiedObject Clone()
		{
			return new EnergySource(this);
		}
	}
}
