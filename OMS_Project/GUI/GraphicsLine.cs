using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace GUI
{
	class GraphicsLine
	{
		public double X1 { get; set; }
		public double Y1 { get; set; }
		public double X2 { get; set; }
		public double Y2 { get; set; }
		public double Thickness { get; set; }

		public Rect AABB
		{
			get
			{
				return new Rect(new Point(X1, Y1), new Point(X2, Y2));
			}
		}
	}
}
