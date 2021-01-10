using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	abstract class TapChanger : PowerSystemResource
	{
		public TapChanger() { }

		public TapChanger(TapChanger t) : base(t)
		{ }
	}
}
