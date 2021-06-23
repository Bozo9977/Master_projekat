using Common.DataModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GUI
{
	public class GraphicsText : IGraphicsElement
	{
		public double X { get; set; }
		public double Y { get; set; }
		public IdentifiedObject IO { get { return null; } }
		public string Text { get; private set; }
		public Brush Foreground { get; private set; }
		public Brush Background { get; private set; }
		public GraphicsElement Element { get; private set; }
		public double FontSize { get; set; }

		public GraphicsText(double x, double y, string text, Brush foreground, Brush background, GraphicsElement element, double fontSize)
		{
			X = x;
			Y = y;
			Text = text;
			Foreground = foreground;
			Background = background;
			Element = element;
			FontSize = fontSize;
		}

		public UIElement[] Draw(ViewTransform vt)
		{
			TextBlock tb = CreateTextBlock();
			TransformGroup tg = new TransformGroup() { Children = new TransformCollection() { new TranslateTransform(X, Y), vt.Transform } };
			tb.RenderTransform = tg;
			return new UIElement[] { tb };
		}

		public Rect AABB
		{
			get
			{
				return Element.AABB;
			}
		}

		public Size CalculateSize()
		{
			TextBlock tb = CreateTextBlock();
			tb.Arrange(new Rect(0, 0, double.MaxValue, double.MaxValue));
			return tb.DesiredSize;
		}

		TextBlock CreateTextBlock()
		{
			return new TextBlock() { Text = Text, Foreground = Foreground, Background = Background, FontSize = FontSize, FontFamily = new FontFamily("Consolas"), FontWeight = FontWeights.Bold, FontStretch = FontStretches.Condensed };
		}
	}
}
