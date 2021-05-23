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
	public interface IGraphicsElement
	{
		double X { get; }
		double Y { get; }
		double Angle { get; }
		double Scale { get; }
		GraphicsModel Model { get; }
		Rect AABB { get; }

		Shape[] Draw();
	}

	public class GraphicsElement : IGraphicsElement
	{
		public double X { get { return Element.X; } }
		public double Y { get { return Element.Y; } }
		public double Angle { get { return 0; } }
		public double Scale { get { return 0.5; } }
		public GraphicsModel Model { get; private set; }
		public IElementLayout Element { get; private set; }
		public Brush Fill { get; private set; }

		public GraphicsElement(IElementLayout element, Brush fill)
		{
			Element = element;
			Model = GraphicsModelMapping.Instance.GetGraphicsModel(Element.IO);
			Fill = fill;
		}

		public Shape[] Draw()
		{
			Shape[] s = Model.Draw();
			ScaleTransform st = new ScaleTransform(Scale, Scale);
			RotateTransform rt = new RotateTransform(Angle);
			TranslateTransform tt = new TranslateTransform(X, Y);

			for(int i = 0; i < s.Length; ++i)
			{
				TransformCollection tc = ((TransformGroup)s[i].RenderTransform).Children;
				tc.Add(st);
				tc.Add(rt);
				tc.Add(tt);
				s[i].Fill = Fill;
			}
			
			return s;
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
