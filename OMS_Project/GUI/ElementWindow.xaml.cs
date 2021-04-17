using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace GUI
{
	/// <summary>
	/// Interaction logic for ElementWindow.xaml
	/// </summary>
	public partial class ElementWindow : Window, IObserver<ObservableMessage>
	{
		public long GID { get; private set; }
		PubSubClient pubSub;

		public ElementWindow(long gid, PubSubClient pubSub)
		{
			InitializeComponent();
			GID = gid;
			this.pubSub = pubSub;
			pubSub.Subscribe(this);
			RefreshInternal();
		}

		public void Notify(ObservableMessage message)
		{
			switch(message.Type)
			{
				case EObservableMessageType.NetworkModelChanged:
					Refresh();
					break;

				case EObservableMessageType.MeasurementValuesChanged:
					Refresh();
					break;
			}
		}

		void Refresh()
		{
			Dispatcher.BeginInvoke(new Action(RefreshInternal));
		}

		void RefreshInternal()
		{
			panel.Children.Clear();
			NetworkModel nm = pubSub.Model;

			IdentifiedObject io = nm.Get(GID);

			if(io == null)
			{
				Title = "Element " + GID;
				panel.Children.Add(new TextBlock() { Margin = new Thickness(5), Text = "The element does not exist.", HorizontalAlignment = HorizontalAlignment.Center });
				return;
			}

			Title = ModelCodeHelper.GetTypeFromGID(GID) + " " + GID;

			if(io is ConductingEquipment)
			{
				HandleConductingEquipment(nm, (ConductingEquipment)io);
			}
		}

		void HandleConductingEquipment(NetworkModel nm, ConductingEquipment io)
		{
			StackPanel measPanel = new StackPanel();

			Border border = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.LightGray, Margin = new Thickness(1), Padding = new Thickness(1) };
			Grid grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			grid.RowDefinitions.Add(new RowDefinition());

			AddToGrid(grid, new TextBlock() { Text = "MRID", FontWeight = FontWeights.Bold }, 0, 0);
			AddToGrid(grid, new TextBlock() { Text = "Direction", FontWeight = FontWeights.Bold }, 0, 2);
			AddToGrid(grid, new TextBlock() { Text = "Value", FontWeight = FontWeights.Bold }, 0, 4);

			int row = 0;

			foreach(long measGID in io.Measurements)
			{
				++row;
				Measurement meas = (Measurement)nm.Get(measGID);

				if(meas == null)
					continue;

				grid.RowDefinitions.Add(new RowDefinition());
				AddToGrid(grid, new TextBlock() { Text = meas.MRID }, row, 0);
				AddToGrid(grid, new TextBlock() { Text = meas.Direction.ToString() }, row, 2);
				AddToGrid(grid, new TextBlock() { Text = ModelCodeHelper.GetTypeFromGID(measGID) == DMSType.Analog ?  pubSub.Measurements.GetAnalogValue(measGID).ToString() : pubSub.Measurements.GetDigitalValue(measGID).ToString() }, row, 4);
			}

			int splitterRowSpan = int.MaxValue;

			if(row == 0)
			{
				grid.RowDefinitions.Add(new RowDefinition());
				Grid.SetColumnSpan(AddToGrid(grid, new TextBlock() { Text = "No measurements.", HorizontalAlignment = HorizontalAlignment.Center }, 1, 0), 5);
				splitterRowSpan = 1;
			}

			Grid.SetRowSpan(AddToGrid(grid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 1), splitterRowSpan);
			Grid.SetRowSpan(AddToGrid(grid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 3), splitterRowSpan);

			grid.RowDefinitions.Last().Height = new GridLength(1, GridUnitType.Star);

			border.Child = grid;
			measPanel.Children.Add(border);
			panel.Children.Add(measPanel);
		}

		UIElement AddToGrid(Grid grid, UIElement element, int row, int column)
		{
			grid.Children.Add(element);
			Grid.SetColumn(element, column);
			Grid.SetRow(element, row);
			return element;
		}
	}
}
