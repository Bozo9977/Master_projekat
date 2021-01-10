using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class Recloser : ProtectedSwitch
	{
		public Recloser() { }

		public Recloser(Recloser r) : base(r)
		{ }

		public override IdentifiedObject Clone()
		{
			return new Recloser(this);
		}
	}
}
