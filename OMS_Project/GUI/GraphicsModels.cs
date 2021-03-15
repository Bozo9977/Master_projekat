using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GUI
{
	public class GraphicsModels
	{
		static Ellipse GetCircle(double r, Brush fill)
		{
			return new Ellipse() { Width = r * 2, Height = r * 2, RenderTransform = new TransformGroup() { Children = new TransformCollection() { new TranslateTransform(-r, -r) } }, Fill = fill };
		}

		public class ConnectivityNode : GraphicsModel
		{
			public override double Radius { get { return 0.4; } }

			public override Shape[] Draw()
			{
				Ellipse e = GetCircle(0.4, Brushes.White);
				e.Stroke = Brushes.Black;
				e.StrokeThickness = Radius / 16;
				return new Shape[] { e };
			}
		}
	}
}
