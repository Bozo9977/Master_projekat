using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GUI
{
	public class GraphicsModels
	{
		const double sqrt2 = 1.4142135623730950488016887242097;
		const double sqrt3div2 = 0.86602540378443864676372317075294;

		static Ellipse GetCircle(double r, Brush fill)
		{
			return new Ellipse() { Width = r * 2, Height = r * 2, RenderTransform = new TransformGroup() { Children = new TransformCollection() { new TranslateTransform(-r, -r) } }, Fill = fill };
		}

		static Rectangle GetRectangle(double w, double h, Brush fill, double x = 0, double y = 0)
		{
			return new Rectangle() { Width = w, Height = h, RenderTransform = new TransformGroup() { Children = new TransformCollection() { new TranslateTransform(-w / 2 + x, -h / 2 + y) } }, Fill = fill };
		}

		static Polygon GetPolygon(PointCollection points, Brush fill)
		{
			return new Polygon() { Points = points, Fill = fill, RenderTransform = new TransformGroup() };
		}

		public class ConnectivityNode : GraphicsModel
		{
			public override double Radius { get { return 0.25; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetCircle(Radius, Brushes.Black) };
			}
		}

		public class EnergyConsumer : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(0, -1), new Point(-sqrt3div2, 0.5), new Point(sqrt3div2, 0.5) }, Brushes.Maroon) };
			}
		}

		public class ResidentialConsumer : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(-sqrt2 / 2, 0), new Point(0, -sqrt2 / 2), new Point(sqrt2 / 2, 0), new Point(sqrt2 / 2, sqrt2 / 2), new Point(-sqrt2 / 2, sqrt2 / 2) }, Brushes.Black) };
			}
		}

		public class AdministrativeConsumer : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(-sqrt2 / 2, 0), new Point(-sqrt2 / 4, 0), new Point(0, -sqrt2 / 2), new Point(sqrt2 / 4, 0), new Point(sqrt2 / 2, 0), new Point(sqrt2 / 2, sqrt2 / 2), new Point(-sqrt2 / 2, sqrt2 / 2) }, Brushes.Black) };
			}
		}

		public class IndustrialConsumer : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(-sqrt2 / 2, -sqrt2 / 2), new Point(0, 0), new Point(0, -sqrt2 / 2), new Point(sqrt2 / 2, 0), new Point(sqrt2 / 2, sqrt2 / 2), new Point(-sqrt2 / 2, sqrt2 / 2) }, Brushes.Black) };
			}
		}

		public class ACLineSegment : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetRectangle(sqrt2 / 2, sqrt2, Brushes.Black) };
			}
		}

		public class Disconnector : GraphicsModel
		{
			public override double Radius { get { return 0.75; } }

			public override Shape[] Draw()
			{
				Shape s = GetRectangle(sqrt2 * Radius, sqrt2 * Radius, Brushes.Black);
				((TransformGroup)s.RenderTransform).Children.Add(new RotateTransform(45));
				return new Shape[] { s };
			}
		}

		public class Breaker : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetRectangle(sqrt2, sqrt2, Brushes.Black) };
			}
		}

		public class Recloser : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				Shape s = GetRectangle(sqrt2, sqrt2, Brushes.Black);
				((TransformGroup)s.RenderTransform).Children.Add(new RotateTransform(45));
				return new Shape[] { GetRectangle(sqrt2, sqrt2, Brushes.Black), s };
			}
		}

		public class DistributionGenerator : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(-sqrt2 / 2, -sqrt2 / 4), new Point(0, -sqrt2 / 2), new Point(sqrt2 / 2, -sqrt2 / 4), new Point(sqrt2 / 2, sqrt2 / 4), new Point(0, sqrt2 / 2), new Point(-sqrt2 / 2, sqrt2 / 4) }, Brushes.Black) };
			}
		}

		public class EnergySource : GraphicsModel
		{
			public override double Radius
			{
				get { return 1; }
			}

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(0, 1), new Point(-sqrt3div2, -0.5), new Point(sqrt3div2, -0.5) }, Brushes.Black) };
			}
		}

		public class TransformerWinding : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(-sqrt2 / 2, -sqrt2 / 2), new Point(sqrt2 / 2, -sqrt2 / 2), new Point(sqrt2 / 2, -sqrt2 / 6), new Point(-sqrt2 / 2, -sqrt2 / 6) }, Brushes.Black), GetPolygon(new PointCollection() { new Point(-sqrt2 / 2, sqrt2 / 6), new Point(sqrt2 / 2, sqrt2 / 6), new Point(sqrt2 / 2, sqrt2 / 2), new Point(-sqrt2 / 2, sqrt2 / 2) }, Brushes.Black) };
			}
		}
	}
}
