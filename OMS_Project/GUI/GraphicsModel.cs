using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GUI
{
	public abstract class GraphicsModel
	{
		public abstract Shape[] Draw();
		public abstract double Radius { get; }
	}
}
