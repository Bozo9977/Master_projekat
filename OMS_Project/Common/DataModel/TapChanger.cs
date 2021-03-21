using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModel
{
	public abstract class TapChanger : PowerSystemResource
	{
		public TapChanger() { }

		public TapChanger(TapChanger t) : base(t)
		{ }

        // validation
        public override bool Validate(Func<long, IdentifiedObject> entityGetter)
        {
            return base.Validate(entityGetter);
        }
    }
}
