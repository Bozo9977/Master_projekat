using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;

namespace CIMXML_Editor
{
	public class NodeModels
	{
		const double sqrt2 = 1.4142135623730950488016887242097;
		const double sqrt3div2 = 0.86602540378443864676372317075294;

		static Ellipse GetCircle(double r, Brush fill)
		{
			return new Ellipse() { Width = r * 2, Height = r * 2, RenderTransform = new TransformGroup() { Children = new TransformCollection() { new TranslateTransform(-r, -r) } }, Fill = fill };
		}

		static Rectangle GetRectangle(double w, double h, Brush fill)
		{
			return new Rectangle() { Width = w, Height = h, RenderTransform = new TransformGroup() { Children = new TransformCollection() { new TranslateTransform(-w/2, -h/2) } }, Fill = fill };
		}

		static Polygon GetPolygon(PointCollection points, Brush fill)
		{
			return new Polygon() { Points = points, Fill = fill, RenderTransform = new TransformGroup() };
		}

		public class ConnectivityNode : NodeModel
		{
			public override double Radius { get { return 0.4; } }

			public override Shape Draw()
			{
				Ellipse e = GetCircle(0.4, Brushes.White);
				e.Stroke = Brushes.Black;
				e.StrokeThickness = Radius / 16;
				return e;
			}
		}

		public class BaseVoltage : NodeModel
		{
			public override double Radius { get { return 1; } }

			public override Shape Draw()
			{
				return GetRectangle(sqrt2, sqrt2, Brushes.DarkCyan);
			}
		}

		public class EnergyConsumer : NodeModel
		{
			public override double Radius { get { return 1; } }

			public override Shape Draw()
			{
				return GetPolygon(new PointCollection() { new Point(0, -1), new Point(-sqrt3div2, 0.5), new Point(sqrt3div2, 0.5) }, Brushes.Maroon);
			}
		}

		public class ACLineSegment : NodeModel
		{
			public override double Radius { get { return 2; } }

			public override Shape Draw()
			{
				return GetRectangle(sqrt2 / 4, sqrt2 * 2, Brushes.Black);
			}
		}

		public class Disconnector : NodeModel
		{
			public override double Radius { get { return 1; } }

			public override Shape Draw()
			{
				return GetRectangle(sqrt2, sqrt2, Brushes.LightBlue);
			}
		}

		public class Breaker : NodeModel
		{
			public override double Radius { get { return 1; } }

			public override Shape Draw()
			{
				return GetRectangle(sqrt2, sqrt2, Brushes.Blue);
			}
		}

		public class Recloser : NodeModel
		{
			public override double Radius { get { return 1; } }

			public override Shape Draw()
			{
				return GetRectangle(sqrt2, sqrt2, Brushes.Purple);
			}
		}

		public class DistributionGenerator : NodeModel
		{
			public override double Radius { get { return 1; } }

			public override Shape Draw()
			{
				return GetRectangle(sqrt2, sqrt2, Brushes.YellowGreen);
			}
		}

		public class PowerTransformer : NodeModel
		{
			public override double Radius { get { return 1; } }

			public override Shape Draw()
			{
				return GetRectangle(sqrt2, sqrt2, Brushes.Gray);
			}
		}

		public class TransformerWinding : NodeModel
		{
			public override double Radius { get { return 1; } }

			public override Shape Draw()
			{
				return GetRectangle(sqrt2, sqrt2, Brushes.DimGray);
			}
		}

		public class RatioTapChanger : NodeModel
		{
			public override double Radius { get { return 1; } }

			public override Shape Draw()
			{
				return GetRectangle(sqrt2, sqrt2, Brushes.SlateGray);
			}
		}

		public class EnergySource : NodeModel
		{
			public override double Radius
			{
				get { return 1; }
			}

			public override Shape Draw()
			{
				return GetPolygon(new PointCollection() { new Point(0, 1), new Point(-sqrt3div2, -0.5), new Point(sqrt3div2, -0.5) }, Brushes.Green);
			}
		}

		public class Terminal : NodeModel
		{
			public override double Radius { get { return 0.3; } }

			public override Shape Draw()
			{
				return GetCircle(0.3, Brushes.Black);
			}
		}

		public class Analog : NodeModel
		{
			public override double Radius { get { return 0.5; } }

			public override Shape Draw()
			{
				return GetRectangle(sqrt2 / 2, sqrt2 / 2, Brushes.Gold);
			}
		}

		public class Discrete : NodeModel
		{
			public override double Radius { get { return 0.5; } }

			public override Shape Draw()
			{
				return GetRectangle(sqrt2 / 2, sqrt2 / 2, Brushes.Brown);
			}
		}
	}
}
