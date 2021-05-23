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

		static Ellipse GetCircle(double r)
		{
			return new Ellipse() { Width = r * 2, Height = r * 2, RenderTransform = new TransformGroup() { Children = new TransformCollection() { new TranslateTransform(-r, -r) } } };
		}

		static Rectangle GetRectangle(double w, double h, double x = 0, double y = 0)
		{
			return new Rectangle() { Width = w, Height = h, RenderTransform = new TransformGroup() { Children = new TransformCollection() { new TranslateTransform(-w / 2 + x, -h / 2 + y) } } };
		}

		static Polygon GetPolygon(PointCollection points)
		{
			return new Polygon() { Points = points, RenderTransform = new TransformGroup() };
		}

		public class ConnectivityNode : GraphicsModel
		{
			public override double Radius { get { return 0.25; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetCircle(Radius) };
			}
		}

		/*public class EnergyConsumer : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(0, -1), new Point(-sqrt3div2, 0.5), new Point(sqrt3div2, 0.5) }) };
			}
		}*/

		public class ResidentialConsumer : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(-sqrt2 / 2, 0), new Point(0, -sqrt2 / 2), new Point(sqrt2 / 2, 0), new Point(sqrt2 / 2, sqrt2 / 2), new Point(-sqrt2 / 2, sqrt2 / 2) }) };
			}
		}

		public class AdministrativeConsumer : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(-sqrt2 / 2, 0), new Point(-sqrt2 / 4, 0), new Point(0, -sqrt2 / 2), new Point(sqrt2 / 4, 0), new Point(sqrt2 / 2, 0), new Point(sqrt2 / 2, sqrt2 / 2), new Point(-sqrt2 / 2, sqrt2 / 2) }) };
			}
		}

		public class IndustrialConsumer : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(-sqrt2 / 2, -sqrt2 / 2), new Point(0, 0), new Point(0, -sqrt2 / 2), new Point(sqrt2 / 2, 0), new Point(sqrt2 / 2, sqrt2 / 2), new Point(-sqrt2 / 2, sqrt2 / 2) }) };
			}
		}

		public class ACLineSegment : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetRectangle(sqrt2 / 2, sqrt2) };
			}
		}

		public class Disconnector : GraphicsModel
		{
			public override double Radius { get { return 0.75; } }

			public override Shape[] Draw()
			{
				Shape s = GetRectangle(sqrt2 * Radius, sqrt2 * Radius);
				((TransformGroup)s.RenderTransform).Children.Add(new RotateTransform(45));
				return new Shape[] { s };
			}
		}

		public class Breaker : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetRectangle(sqrt2, sqrt2) };
			}
		}

		public class Recloser : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				Shape s = GetRectangle(sqrt2, sqrt2);
				((TransformGroup)s.RenderTransform).Children.Add(new RotateTransform(45));
				return new Shape[] { GetRectangle(sqrt2, sqrt2), s };
			}
		}

		public class DistributionGenerator : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(-sqrt2 / 2, -sqrt2 / 4), new Point(0, -sqrt2 / 2), new Point(sqrt2 / 2, -sqrt2 / 4), new Point(sqrt2 / 2, sqrt2 / 4), new Point(0, sqrt2 / 2), new Point(-sqrt2 / 2, sqrt2 / 4) }) };
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
				return new Shape[] { GetPolygon(new PointCollection() { new Point(0, 1), new Point(-sqrt3div2, -0.5), new Point(sqrt3div2, -0.5) }) };
			}
		}

		public class TransformerWinding : GraphicsModel
		{
			public override double Radius { get { return 1; } }

			public override Shape[] Draw()
			{
				return new Shape[] { GetPolygon(new PointCollection() { new Point(-sqrt2 / 2, -sqrt2 / 2), new Point(sqrt2 / 2, -sqrt2 / 2), new Point(sqrt2 / 2, -sqrt2 / 6), new Point(-sqrt2 / 2, -sqrt2 / 6) }), GetPolygon(new PointCollection() { new Point(-sqrt2 / 2, sqrt2 / 6), new Point(sqrt2 / 2, sqrt2 / 6), new Point(sqrt2 / 2, sqrt2 / 2), new Point(-sqrt2 / 2, sqrt2 / 2) }) };
			}
		}
	}
}
