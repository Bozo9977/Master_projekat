using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class Disconnector : Switch
	{
		public Disconnector() { }

		public Disconnector(Disconnector d) : base(d)
		{ }

		public override IdentifiedObject Clone()
		{
			return new Disconnector(this);
		}
	}
}
