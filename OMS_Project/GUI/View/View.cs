using System.Windows;
using System.Windows.Controls;

namespace GUI.View
{
	public abstract class View
	{
		public StackPanel Panel { get; private set; }

		public View()
		{
			Panel = new StackPanel();
		}

		public abstract void Refresh();

		protected UIElement AddToGrid(Grid grid, UIElement element, int row, int column)
		{
			grid.Children.Add(element);
			Grid.SetColumn(element, column);
			Grid.SetRow(element, row);
			return element;
		}
	}
}
