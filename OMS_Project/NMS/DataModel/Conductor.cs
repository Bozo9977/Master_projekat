using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	abstract class Conductor : ConductingEquipment
	{
		public Conductor() { }

		public Conductor(Conductor c) : base(c)
		{ }
	}
}
