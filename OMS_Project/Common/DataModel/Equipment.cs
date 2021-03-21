using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModel
{
	public abstract class Equipment : PowerSystemResource
	{
		public Equipment() { }

		public Equipment(Equipment e) : base(e)
		{ }

        // validation
        public override bool Validate(Func<long, IdentifiedObject> entityGetter)
        {
            return base.Validate(entityGetter);
        }
    }
}
