using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace GUI
{
	interface IGraphicsLine
	{
		double X1 { get; }
		double Y1 { get; }
		double X2 { get; }
		double Y2 { get; }
		double Thickness { get; }
		Rect AABB { get; }
		Brush Fill { get; }
	}

	class GraphicsLine : IGraphicsLine
	{
		public double X1 { get; set; }
		public double Y1 { get; set; }
		public double X2 { get; set; }
		public double Y2 { get; set; }
		public double Thickness { get; set; }
		public Brush Fill { get; set; }

		public GraphicsLine(IElementLayout element1, IElementLayout element2, Brush fill)
		{
			X1 = element1.X;
			Y1 = element1.Y;
			X2 = element2.X;
			Y2 = element2.Y;
			Thickness = 2;
			Fill = fill;
		}

		public Rect AABB
		{
			get
			{
				return new Rect(new Point(X1, Y1), new Point(X2, Y2));
			}
		}
	}
}
