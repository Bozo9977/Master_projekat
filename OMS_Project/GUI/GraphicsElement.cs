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
	public class GraphicsElement
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Angle { get; set; }
		public double Scale { get; set; }
		public GraphicsModel Model { get; set; }

		public Shape[] Draw()
		{
			Shape[] s = Model.Draw();

			for(int i = 0; i < s.Length; ++i)
			{
				TransformCollection tc = ((TransformGroup)s[i].RenderTransform).Children;
				tc.Add(new ScaleTransform(Scale, Scale));
				tc.Add(new RotateTransform(Angle));
				tc.Add(new TranslateTransform(X, Y));
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
