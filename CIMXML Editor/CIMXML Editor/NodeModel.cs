using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Shapes;

namespace CIMXML_Editor
{
	public abstract class NodeModel
	{
		public abstract Shape Draw();
		public abstract double Radius { get; }
	}
}
