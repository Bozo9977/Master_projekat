using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GUI.View
{
	public abstract class View
	{
		public abstract UIElement Element { get; }
		public abstract void Update(EObservableMessageType msg);
		public abstract void Update();

		protected UIElement AddToGrid(Grid grid, UIElement element, int row, int column)
		{
			grid.Children.Add(element);
			Grid.SetColumn(element, column);
			Grid.SetRow(element, row);
			return element;
		}

		protected TextBlock CreateHyperlink(string text, Action action)
		{
			TextBlock tb = new TextBlock() { Text = text, Foreground = Brushes.Blue, TextDecorations = TextDecorations.Underline, Cursor = Cursors.Hand };
			tb.MouseLeftButtonUp += (x, y) => action();
			return tb;
		}

		protected TextBox CreateLabel(string text)
		{
			return new TextBox() { Text = text, IsReadOnly = true, TextWrapping = TextWrapping.Wrap, BorderThickness = new Thickness(0) };
		}
	}

	public class NullView : View
	{
		TextBlock tb;
		public override UIElement Element { get { return tb; } }

		public NullView()
		{
			tb = new TextBlock() { Margin = new Thickness(5), Text = "The element does not exist.", HorizontalAlignment = HorizontalAlignment.Center };
		}

		public override void Update(EObservableMessageType msg)
		{ }

		public override void Update()
		{ }
	}
}
