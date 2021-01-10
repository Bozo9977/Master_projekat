using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class Breaker : ProtectedSwitch
	{
		public Breaker() { }

		public Breaker(Breaker b) : base(b)
		{ }

		public override IdentifiedObject Clone()
		{
			return new Breaker(this);
		}
	}
}
