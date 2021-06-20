using Common.DataModel;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GUI
{
	public interface IGraphicsElement
	{
		Rect AABB { get; }
		IdentifiedObject IO { get; }

		UIElement[] Draw(ViewTransform vt);
	}

	public class GraphicsElement : IGraphicsElement
	{
		public double X { get { return Element.X; } }
		public double Y { get { return Element.Y; } }
		public double Angle { get { return 0; } }
		public double Scale { get { return 0.5; } }
		public GraphicsModel Model { get; private set; }
		public IElementLayout Element { get; private set; }
		public IdentifiedObject IO { get { return Element == null ? null : Element.IO; } }
		public Brush Fill { get; private set; }

		public GraphicsElement(IElementLayout element, Brush fill)
		{
			Element = element;
			Model = GraphicsModelMapping.Instance.GetGraphicsModel(Element.IO);
			Fill = fill;
		}

		public UIElement[] Draw(ViewTransform vt)
		{
			Shape[] shapes = Model.Draw();
			TransformGroup tg = new TransformGroup() { Children = new TransformCollection() { new ScaleTransform(Scale, Scale), new RotateTransform(Angle), new TranslateTransform(X, Y), vt.Transform } };

			for(int i = 0; i < shapes.Length; ++i)
			{
				Shape shape = shapes[i];
				((TransformGroup)shape.RenderTransform).Children.Add(tg);
				shape.Fill = Fill;
			}
			
			return shapes;
		}

		public Rect AABB
		{
			get
			{
				double r = Model.Radius * Scale;
				return new Rect(X - r, Y - r, r * 2, r * 2);
			}
		}
	}
}
