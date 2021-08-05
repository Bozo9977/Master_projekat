using Common.DataModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GUI
{
	class GraphicsLine : IGraphicsElement
	{
		public double X1 { get; set; }
		public double Y1 { get; set; }
		public double X2 { get; set; }
		public double Y2 { get; set; }
		public double Thickness { get; set; }
		public Brush Fill { get; set; }
		public IdentifiedObject IO { get { return null; } }
		public double MinZoom { get { return double.MinValue; } }

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

		public UIElement[] Draw(ViewTransform vt)
		{
			Point p1 = vt.Transform.Transform(new Point(X1, Y1));
			Point p2 = vt.Transform.Transform(new Point(X2, Y2));

			return new UIElement[] { new Line() { Stroke = Fill, StrokeThickness = Thickness, X1 = p1.X, Y1 = p1.Y, X2 = p2.X, Y2 = p2.Y } };
		}
	}
}
