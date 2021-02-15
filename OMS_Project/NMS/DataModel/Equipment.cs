using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	public abstract class Equipment : PowerSystemResource
	{
		public Equipment() { }

		public Equipment(Equipment e) : base(e)
		{ }
	}
}
