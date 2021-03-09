using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModel
{
	public abstract class Conductor : ConductingEquipment
	{
		public Conductor() { }

		public Conductor(Conductor c) : base(c)
		{ }


        //validation
        public override void GetEntitiesToValidate(Func<long, IdentifiedObject> entityGetter, HashSet<long> dst)
        {
            base.GetEntitiesToValidate(entityGetter, dst);
        }

        public override bool Validate(Func<long, IdentifiedObject> entityGetter)
        {
            return base.Validate(entityGetter);
        }
    }
}
