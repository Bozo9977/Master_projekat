using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	abstract class ProtectedSwitch : Switch
	{
		public ProtectedSwitch() { }

		public ProtectedSwitch(ProtectedSwitch p) : base(p)
		{ }
	}
}
