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
		bool initialized;

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
			NetworkModel nm = pubSub.Model;
			IdentifiedObject io = nm.Get(GID);

			if(io == null)
			{
				panel.Children.Clear();
				Title = "Element " + GID;
				panel.Children.Add(new TextBlock() { Margin = new Thickness(5), Text = "The element does not exist.", HorizontalAlignment = HorizontalAlignment.Center });
				initialized = false;
				return;
			}

			Title = ModelCodeHelper.GetTypeFromGID(GID) + " " + GID;

			if(io is ConductingEquipment)
			{
				if(!initialized)
				{
					InitConductingEquipment(nm, (ConductingEquipment)io);
					initialized = true;
				}

				UpdateConductingEquipment(nm, (ConductingEquipment)io);
			}
			else if(io is ConnectivityNode)
			{
				if(!initialized)
				{
					InitConnectivityNode(nm, (ConnectivityNode)io);
					initialized = true;
				}

				UpdateConnectivityNode(nm, (ConnectivityNode)io);
			}
		}

		void InitConnectivityNode(NetworkModel nm, ConnectivityNode io)
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

			AddToGrid(grid, new TextBlock() { Text = "MRID", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 0);
			AddToGrid(grid, new TextBlock() { Text = "Direction", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 2);
			AddToGrid(grid, new TextBlock() { Text = "Value", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 4);

			AddToGrid(grid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 1);
			AddToGrid(grid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 3);

			border.Child = grid;
			measPanel.Children.Add(border);
			panel.Children.Clear();
			panel.Children.Add(new TextBlock() { Margin = new Thickness(2), Text = "Measurements", FontWeight = FontWeights.Bold, FontSize = 14 });
			panel.Children.Add(measPanel);
		}

		void UpdateConnectivityNode(NetworkModel nm, ConnectivityNode io)
		{
			Grid grid = (Grid)((Border)((StackPanel)panel.Children[1]).Children[0]).Child;

			if(grid.Children.Count > 5)
				grid.Children.RemoveRange(5, grid.Children.Count - 5);

			if(grid.RowDefinitions.Count > 1)
				grid.RowDefinitions.RemoveRange(1, grid.RowDefinitions.Count - 1);

			int row = 0;

			foreach(long terminalGID in io.Terminals)
			{
				Terminal terminal = (Terminal)nm.Get(terminalGID);

				if(terminal == null)
					continue;

				foreach(long measGID in terminal.Measurements)
				{
					++row;
					Measurement meas = (Measurement)nm.Get(measGID);

					if(meas == null)
						continue;

					grid.RowDefinitions.Add(new RowDefinition());
					AddToGrid(grid, new TextBlock() { Text = meas.MRID }, row, 0);
					AddToGrid(grid, new TextBlock() { Text = meas.Direction.ToString() }, row, 2);

					string valueText;
					bool isNormal;

					if(ModelCodeHelper.GetTypeFromGID(measGID) == DMSType.Analog)
					{
						float value = pubSub.Measurements.GetAnalogValue(measGID);
						Analog a = (Analog)meas;

						isNormal = value <= a.MaxValue && value >= a.MinValue;
						valueText = value.ToString();
					}
					else
					{
						int value = pubSub.Measurements.GetDiscreteValue(measGID);
						Discrete d = (Discrete)meas;

						isNormal = value <= d.MaxValue && value >= d.MinValue;
						valueText = value.ToString();
					}

					AddToGrid(grid, new TextBlock() { Text = valueText, Foreground = isNormal ? Brushes.Black : Brushes.White, Background = isNormal ? Brushes.White : Brushes.Red, FontWeight = isNormal ? FontWeights.Regular : FontWeights.Bold, TextAlignment = TextAlignment.Right }, row, 4);
				}
			}

			int splitterRowSpan = int.MaxValue;

			if(row == 0)
			{
				grid.RowDefinitions.Add(new RowDefinition());
				Grid.SetColumnSpan(AddToGrid(grid, new TextBlock() { Text = "No measurements.", HorizontalAlignment = HorizontalAlignment.Center }, 1, 0), 5);
				splitterRowSpan = 1;
			}

			Grid.SetRowSpan(grid.Children[3], splitterRowSpan);
			Grid.SetRowSpan(grid.Children[4], splitterRowSpan);

			grid.RowDefinitions.Last().Height = new GridLength(1, GridUnitType.Star);
		}

		void InitConductingEquipment(NetworkModel nm, ConductingEquipment io)
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

			AddToGrid(grid, new TextBlock() { Text = "MRID", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 0);
			AddToGrid(grid, new TextBlock() { Text = "Direction", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 2);
			AddToGrid(grid, new TextBlock() { Text = "Value", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 4);

			AddToGrid(grid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 1);
			AddToGrid(grid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 3);

			border.Child = grid;
			measPanel.Children.Add(border);
			panel.Children.Clear();
			panel.Children.Add(new TextBlock() { Margin = new Thickness(2), Text = "Measurements", FontWeight = FontWeights.Bold, FontSize = 14 });
			panel.Children.Add(measPanel);
		}

		void UpdateConductingEquipment(NetworkModel nm, ConductingEquipment io)
		{
			Grid grid = (Grid)((Border)((StackPanel)panel.Children[1]).Children[0]).Child;

			if(grid.Children.Count > 5)
				grid.Children.RemoveRange(5, grid.Children.Count - 5);

			if(grid.RowDefinitions.Count > 1)
				grid.RowDefinitions.RemoveRange(1, grid.RowDefinitions.Count - 1);

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

				string valueText;
				bool isNormal;

				if(ModelCodeHelper.GetTypeFromGID(measGID) == DMSType.Analog)
				{
					float value = pubSub.Measurements.GetAnalogValue(measGID);
					Analog a = (Analog)meas;

					isNormal = value <= a.MaxValue && value >= a.MinValue;
					valueText = value.ToString();
				}
				else
				{
					int value = pubSub.Measurements.GetDiscreteValue(measGID);
					Discrete d = (Discrete)meas;

					isNormal = value <= d.MaxValue && value >= d.MinValue;
					valueText = value.ToString();
				}

				AddToGrid(grid, new TextBlock() { Text = valueText, Foreground = isNormal ? Brushes.Black : Brushes.White, Background = isNormal ? Brushes.White : Brushes.Red, FontWeight = isNormal ? FontWeights.Regular : FontWeights.Bold, TextAlignment = TextAlignment.Right }, row, 4);
			}

			int splitterRowSpan = int.MaxValue;

			if(row == 0)
			{
				grid.RowDefinitions.Add(new RowDefinition());
				Grid.SetColumnSpan(AddToGrid(grid, new TextBlock() { Text = "No measurements.", HorizontalAlignment = HorizontalAlignment.Center }, 1, 0), 5);
				splitterRowSpan = 1;
			}

			Grid.SetRowSpan(grid.Children[3], splitterRowSpan);
			Grid.SetRowSpan(grid.Children[4], splitterRowSpan);

			grid.RowDefinitions.Last().Height = new GridLength(1, GridUnitType.Star);
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
