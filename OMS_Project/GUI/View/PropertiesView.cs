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
	public class PropertiesView : View
	{
		Func<IdentifiedObject> ioGetter;
		PubSubClient pubSub;
		Dictionary<DMSType, List<ModelCode>> typeToProps;
		bool initialized;
		StackPanel panel;

		public override UIElement Element
		{
			get
			{
				if(!initialized)
					Update();

				return panel;
			}
		}

		public PropertiesView(Func<IdentifiedObject> ioGetter, PubSubClient pubSub) : base()
		{
			this.ioGetter = ioGetter;
			this.pubSub = pubSub;
			typeToProps = ModelResourcesDesc.GetTypeToPropertiesMap();
			panel = new StackPanel();

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
			panel.Children.Add(new TextBlock() { Margin = new Thickness(2), Text = "Properties", FontWeight = FontWeights.Bold, FontSize = 14 });
			panel.Children.Add(propPanel);
		}

		public override void Update(EObservableMessageType msg)
		{
			if(initialized && msg != EObservableMessageType.NetworkModelChanged)
				return;

			Update();
		}

		public override void Update()
		{
			Grid grid = (Grid)((Border)((StackPanel)panel.Children[1]).Children[0]).Child;

			if(grid.Children.Count > 3)
				grid.Children.RemoveRange(3, grid.Children.Count - 3);

			if(grid.RowDefinitions.Count > 1)
				grid.RowDefinitions.RemoveRange(1, grid.RowDefinitions.Count - 1);

			IdentifiedObject io = ioGetter();
			
			List<ModelCode> props;
			if(io == null || !typeToProps.TryGetValue(ModelCodeHelper.GetTypeFromGID(io.GID), out props))
			{
				Grid.SetColumnSpan(AddToGrid(grid, new TextBlock() { Text = "No properties." }, 1, 0), int.MaxValue);
				Grid.SetRowSpan(grid.Children[2], 1);
				return;
			}

			List<KeyValuePair<string, UIElement>> rows = new List<KeyValuePair<string, UIElement>>();

			foreach(ModelCode prop in props)
			{
				Property p = io.GetProperty(prop);

				if(p == null)
					continue;

				UIElement element;

				switch(p.Type)
				{
					case PropertyType.Bool:
						element = new TextBlock() { Text = ((BoolProperty)p).Value.ToString(), TextAlignment = TextAlignment.Right };
						break;

					case PropertyType.Enum:
						element = new TextBlock() { Text = ((EnumProperty)p).Value.ToString(), TextAlignment = TextAlignment.Right };
						break;

					case PropertyType.Float:
						element = new TextBlock() { Text = ((FloatProperty)p).Value.ToString(), TextAlignment = TextAlignment.Right };
						break;

					case PropertyType.Int32:
						element = new TextBlock() { Text = ((Int32Property)p).Value.ToString(), TextAlignment = TextAlignment.Right };
						break;

					case PropertyType.Int64:
						element = new TextBlock() { Text = ((Int64Property)p).Value.ToString(), TextAlignment = TextAlignment.Right };
						break;

					case PropertyType.Reference:
					{
						long value = ((ReferenceProperty)p).Value;

						if(value != 0)
						{
							TextBlock tb = CreateHyperlink(value.ToString(), () => new ElementWindow(value, pubSub) { Owner = Application.Current.MainWindow }.Show());
							tb.TextAlignment = TextAlignment.Right;
							element = tb;
						}
						else
						{
							element = new TextBlock() { Text = value.ToString(), TextAlignment = TextAlignment.Right };
						}
					}
					break;

					case PropertyType.String:
						element = new TextBlock(){ Text = ((StringProperty)p).Value.ToString(), TextAlignment = TextAlignment.Right };
						break;

					case PropertyType.ReferenceVector:
					{
						List<long> values = ((ReferencesProperty)p).Value;
						StackPanel sp = new StackPanel();
						
						for(int j = 0; j < values.Count; ++j)
						{
							long value = values[j];
							TextBlock tb = CreateHyperlink(value.ToString(), () => new ElementWindow(value, pubSub) { Owner = Application.Current.MainWindow }.Show());
							tb.TextAlignment = TextAlignment.Right;
							sp.Children.Add(tb);
						}

						element = new ScrollViewer() { MaxHeight = 100, Content = sp, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
					}
					break;

					default:
						continue;
				}

				rows.Add(new KeyValuePair<string, UIElement>(prop.ToString(), element));
			}

			rows.Sort((x, y) => { return x.Key.CompareTo(y.Key); });

			int i;
			for(i = 0; i < rows.Count; ++i)
			{
				var row = rows[i];
				grid.RowDefinitions.Add(new RowDefinition());
				AddToGrid(grid, new TextBlock() { Text = row.Key }, i + 1, 0);
				AddToGrid(grid, row.Value, i + 1, 2);
			}

			if(i == 0)
			{
				Grid.SetColumnSpan(AddToGrid(grid, new TextBlock() { Text = "No properties." }, 1, 0), int.MaxValue);
				Grid.SetRowSpan(grid.Children[2], 1);
			}
			else
			{
				Grid.SetRowSpan(grid.Children[2], int.MaxValue);
			}

			initialized = true;
		}
	}
}
