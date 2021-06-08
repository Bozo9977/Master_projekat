using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GUI.View
{
	public class ElementView
	{
		protected StackPanel panel;
		protected PubSubClient pubSub;
		bool initialized;

		public ElementView(StackPanel panel, PubSubClient pubSub)
		{
			this.panel = panel;
			this.pubSub = pubSub;
		}

		public virtual void Refresh(IdentifiedObject io)
		{
			if(!initialized)
			{
				Init(io);
				initialized = true;
			}

			RefreshInternal(io);
		}

		void Init(IdentifiedObject io)
		{
			StackPanel propPanel = new StackPanel();
			Border border = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.LightGray, Margin = new Thickness(1), Padding = new Thickness(1) };
			Grid propsGrid = new Grid();
			propsGrid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			propsGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
			propsGrid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			propsGrid.RowDefinitions.Add(new RowDefinition());

			AddToGrid(propsGrid, new TextBlock() { Text = "Name", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 0);
			AddToGrid(propsGrid, new TextBlock() { Text = "Value", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 2);

			AddToGrid(propsGrid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 1);

			border.Child = propsGrid;
			propPanel.Children.Add(border);
			panel.Children.Clear();
			panel.Children.Add(new TextBlock() { Margin = new Thickness(2), Text = "Properties", FontWeight = FontWeights.Bold, FontSize = 14 });
			panel.Children.Add(propPanel);
		}

		void RefreshInternal(IdentifiedObject io)
		{
			RefreshProperties((StackPanel)panel.Children[1], io);
		}

		protected UIElement AddToGrid(Grid grid, UIElement element, int row, int column)
		{
			grid.Children.Add(element);
			Grid.SetColumn(element, column);
			Grid.SetRow(element, row);
			return element;
		}

		protected void RefreshProperties(StackPanel propsPanel, IdentifiedObject io)
		{
			Grid grid = (Grid)((Border)propsPanel.Children[0]).Child;

			if(grid.Children.Count > 3)
				grid.Children.RemoveRange(3, grid.Children.Count - 3);

			if(grid.RowDefinitions.Count > 1)
				grid.RowDefinitions.RemoveRange(1, grid.RowDefinitions.Count - 1);

			DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);
			Dictionary<DMSType, List<ModelCode>> typeToProps = ModelResourcesDesc.GetTypeToPropertiesMap();
			List<ModelCode> props;

			if(!typeToProps.TryGetValue(type, out props))
			{
				Grid.SetColumnSpan(AddToGrid(grid, new TextBlock() { Text = "No properties." }, 1, 0), int.MaxValue);
				Grid.SetRowSpan(grid.Children[2], 1);
				return;
			}

			List<Tuple<string, object, PropertyType>> values = new List<Tuple<string, object, PropertyType>>();

			foreach(ModelCode prop in props)
			{
				Property p = io.GetProperty(prop);

				if(p == null)
					continue;

				object value;

				switch(p.Type)
				{
					case PropertyType.Bool:
						value = ((BoolProperty)p).Value;
						break;

					case PropertyType.Enum:
						value = ((EnumProperty)p).Value;
						break;

					case PropertyType.Float:
						value = ((FloatProperty)p).Value;
						break;

					case PropertyType.Int32:
						value = ((Int32Property)p).Value;
						break;

					case PropertyType.Int64:
						value = ((Int64Property)p).Value;
						break;

					case PropertyType.Reference:
						value = ((ReferenceProperty)p).Value;
						break;

					case PropertyType.String:
						value = ((StringProperty)p).Value;
						break;

					default:
						continue;
				}

				values.Add(new Tuple<string, object, PropertyType>(prop.ToString(), value, p.Type));
			}

			values.Sort((x, y) => { return x.Item1.CompareTo(y.Item1); });
			int row = 1;

			foreach(Tuple<string, object, PropertyType> value in values)
			{
				grid.RowDefinitions.Add(new RowDefinition());
				AddToGrid(grid, new TextBlock() { Text = value.Item1 }, row, 0);

				TextBlock tbValue;

				if(value.Item3 == PropertyType.Reference && (long)value.Item2 != 0)
				{
					tbValue = new TextBlock() { Text = value.Item2.ToString(), Foreground = Brushes.Blue, TextDecorations = TextDecorations.Underline, Cursor = Cursors.Hand, TextAlignment = TextAlignment.Right };
					tbValue.MouseLeftButtonDown += (x, y) => new ElementWindow((long)value.Item2, pubSub).ShowDialog();
				}
				else
				{
					tbValue = new TextBlock() { Text = value.Item2.ToString(), TextAlignment = TextAlignment.Right };
				}

				AddToGrid(grid, tbValue, row, 2);
				++row;
			}

			if(row == 1)
			{
				Grid.SetColumnSpan(AddToGrid(grid, new TextBlock() { Text = "No properties." }, 1, 0), int.MaxValue);
				Grid.SetRowSpan(grid.Children[2], 1);
				return;
			}

			Grid.SetRowSpan(grid.Children[2], int.MaxValue);
		}
	}
}
