using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GUI.View
{
	public class MeasurementsView : ElementView
	{
		Func<IdentifiedObject, IEnumerable<long>> measurementsGetter;

		public MeasurementsView(IdentifiedObject io, Func<IdentifiedObject, IEnumerable<long>> measurementsGetter, PubSubClient pubSub) : base(io, pubSub)
		{
			this.measurementsGetter = measurementsGetter;
			StackPanel measPanel = new StackPanel();
			Border border = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.LightGray, Margin = new Thickness(1), Padding = new Thickness(1) };
			Grid grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			grid.RowDefinitions.Add(new RowDefinition());

			AddToGrid(grid, new TextBlock() { Text = "MRID", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 0);
			AddToGrid(grid, new TextBlock() { Text = "Direction", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 2);
			AddToGrid(grid, new TextBlock() { Text = "Value", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 4);

			AddToGrid(grid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 1);
			AddToGrid(grid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 3);

			border.Child = grid;
			measPanel.Children.Add(border);
			Panel.Children.Add(new TextBlock() { Margin = new Thickness(2), Text = "Measurements", FontWeight = FontWeights.Bold, FontSize = 14 });
			Panel.Children.Add(measPanel);
		}

		public override void Refresh()
		{
			Grid measGrid = (Grid)((Border)((StackPanel)Panel.Children[1]).Children[0]).Child;

			if(measGrid.Children.Count > 5)
				measGrid.Children.RemoveRange(5, measGrid.Children.Count - 5);

			if(measGrid.RowDefinitions.Count > 1)
				measGrid.RowDefinitions.RemoveRange(1, measGrid.RowDefinitions.Count - 1);

			int row = 0;
			NetworkModel nm = PubSub.Model;

			foreach(long measGID in measurementsGetter(IO))
			{
				++row;
				Measurement meas = (Measurement)nm.Get(measGID);

				if(meas == null)
					continue;

				measGrid.RowDefinitions.Add(new RowDefinition());
				TextBlock mridTextBlock = new TextBlock() { Text = meas.MRID, Foreground = Brushes.Blue, TextDecorations = TextDecorations.Underline, Cursor = Cursors.Hand };
				mridTextBlock.MouseLeftButtonDown += (x, y) => new ElementWindow(measGID, PubSub) { Owner = Application.Current.MainWindow }.Show();
				AddToGrid(measGrid, mridTextBlock, row, 0);
				AddToGrid(measGrid, new TextBlock() { Text = meas.Direction.ToString() }, row, 2);

				string valueText;
				bool isNormal;

				if(ModelCodeHelper.GetTypeFromGID(measGID) == DMSType.Analog)
				{
					float value;
					bool available = PubSub.Measurements.GetAnalogInput(measGID, out value);
					Analog a = (Analog)meas;

					isNormal = !available || (value <= a.MaxValue && value >= a.MinValue);
					valueText = available ? value.ToString() : "N/A";
				}
				else
				{
					int value;
					bool available = PubSub.Measurements.GetDiscreteInput(measGID, out value);
					Discrete d = (Discrete)meas;

					isNormal = !available || (value <= d.MaxValue && value >= d.MinValue);
					valueText = available ? value.ToString() : "N/A";
				}

				AddToGrid(measGrid, new TextBlock() { Text = valueText, Foreground = isNormal ? Brushes.Black : Brushes.White, Background = isNormal ? Brushes.White : Brushes.Red, FontWeight = isNormal ? FontWeights.Regular : FontWeights.Bold, TextAlignment = TextAlignment.Right }, row, 4);
			}

			int splitterRowSpan = int.MaxValue;

			if(row == 0)
			{
				measGrid.RowDefinitions.Add(new RowDefinition());
				Grid.SetColumnSpan(AddToGrid(measGrid, new TextBlock() { Text = "No measurements.", HorizontalAlignment = HorizontalAlignment.Center }, 1, 0), 5);
				splitterRowSpan = 1;
			}

			Grid.SetRowSpan(measGrid.Children[3], splitterRowSpan);
			Grid.SetRowSpan(measGrid.Children[4], splitterRowSpan);

			measGrid.RowDefinitions.Last().Height = new GridLength(1, GridUnitType.Star);
		}
	}
}
