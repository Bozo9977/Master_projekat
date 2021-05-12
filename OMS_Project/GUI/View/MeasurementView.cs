﻿using Common.DataModel;
using Common.GDA;
using Common.SCADA;
using Common.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace GUI.View
{
	public class MeasurementView : ElementView
	{
		bool initialized;

		public MeasurementView(StackPanel panel, PubSubClient pubSub) : base(panel, pubSub)
		{ }

		public override void Refresh(IdentifiedObject io)
		{
			if(!initialized)
			{
				Init((Measurement)io);
				initialized = true;
			}

			RefreshInternal((Measurement)io);
		}

		void Init(Measurement io)
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

			StackPanel measPanel = new StackPanel();
			border = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.LightGray, Margin = new Thickness(1), Padding = new Thickness(1) };
			Grid measGrid = new Grid();
			measGrid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			measGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
			measGrid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			measGrid.RowDefinitions.Add(new RowDefinition());

			AddToGrid(measGrid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 1);

			border.Child = measGrid;
			measPanel.Children.Add(border);
			panel.Children.Add(new TextBlock() { Margin = new Thickness(2), Text = "Values", FontWeight = FontWeights.Bold, FontSize = 14 });
			panel.Children.Add(measPanel);

			StackPanel cmdPanel = new StackPanel();
			border = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.LightGray, Margin = new Thickness(1), Padding = new Thickness(1) };
			Grid cmdGrid = new Grid();
			cmdGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(3, GridUnitType.Star) });
			cmdGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
			cmdGrid.RowDefinitions.Add(new RowDefinition());

			TextBox tbCommand = new TextBox();
			AddToGrid(cmdGrid, tbCommand, 0, 0);
			Button btCommand = new Button() { Content = "Execute" };
			btCommand.Click += (x, y) => ExecuteCommand(io, tbCommand.Text);
			AddToGrid(cmdGrid, btCommand, 0, 1);

			border.Child = cmdGrid;
			cmdPanel.Children.Add(border);
			panel.Children.Add(new TextBlock() { Margin = new Thickness(2), Text = "Command", FontWeight = FontWeights.Bold, FontSize = 14 });
			panel.Children.Add(cmdPanel);
		}

		void ExecuteCommand(Measurement m, string text)
		{
			DMSType type = ModelCodeHelper.GetTypeFromGID(m.GID);

			if(type == DMSType.Analog)
			{
				float value;

				if(!float.TryParse(text, out value))
					return;

				Client<ISCADAServiceContract> client = new Client<ISCADAServiceContract>("endpointSCADA");
				client.Connect();
				client.Call<bool>(scada => { scada.CommandAnalog(new List<long>(1) { m.GID }, new List<float>(1) { value }); return true; }, out _);
				client.Disconnect();
			}
			else if(type == DMSType.Discrete)
			{
				int value;

				if(!int.TryParse(text, out value))
					return;

				Client<ISCADAServiceContract> client = new Client<ISCADAServiceContract>("endpointSCADA");
				client.Connect();
				client.Call<bool>(scada => { scada.CommandDiscrete(new List<long>(1) { m.GID }, new List<int>(1) { value }); return true; }, out _);
				client.Disconnect();
			}
		}

		void RefreshInternal(Measurement io)
		{
			RefreshProperties((StackPanel)panel.Children[1], io);
			RefreshMeas((StackPanel)panel.Children[3], io);
		}

		void RefreshMeas(StackPanel measPanel, Measurement io)
		{
			Grid grid = (Grid)((Border)measPanel.Children[0]).Child;

			if(grid.Children.Count > 1)
				grid.Children.RemoveRange(1, grid.Children.Count - 1);

			grid.RowDefinitions.Clear();

			string inputValue, outputValue;
			bool inputNormal, outputNormal;
			
			DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);

			if(type == DMSType.Analog)
			{
				Analog a = (Analog)io;
				float value;

				if(pubSub.Measurements.GetAnalogInput(io.GID, out value))
				{
					inputNormal = value <= a.MaxValue && value >= a.MinValue;
					inputValue = value.ToString();
				}
				else
				{
					inputNormal = true;
					inputValue = "N/A";
				}

				if(pubSub.Measurements.GetAnalogOutput(io.GID, out value))
				{
					outputNormal = value <= a.MaxValue && value >= a.MinValue;
					outputValue = value.ToString();
				}
				else
				{
					outputNormal = true;
					outputValue = "N/A";
				}
			}
			else if(type == DMSType.Discrete)
			{
				Discrete d = (Discrete)io;
				int value;

				if(pubSub.Measurements.GetDiscreteInput(io.GID, out value))
				{
					inputNormal = value <= d.MaxValue && value >= d.MinValue;
					inputValue = value.ToString();
				}
				else
				{
					inputNormal = true;
					inputValue = "N/A";
				}

				if(pubSub.Measurements.GetDiscreteOutput(io.GID, out value))
				{
					outputNormal = value <= d.MaxValue && value >= d.MinValue;
					outputValue = value.ToString();
				}
				else
				{
					outputNormal = true;
					outputValue = "N/A";
				}
			}
			else
			{
				return;
			}

			int row = 0;
			grid.RowDefinitions.Add(new RowDefinition());
			AddToGrid(grid, new TextBlock() { Text = "Value" }, row, 0);
			AddToGrid(grid, new TextBlock() { Text = inputValue, Foreground = inputNormal ? Brushes.Black : Brushes.Red, FontWeight = inputNormal ? FontWeights.Regular : FontWeights.Bold, TextAlignment = TextAlignment.Right }, row++, 2);

			grid.RowDefinitions.Add(new RowDefinition());
			AddToGrid(grid, new TextBlock() { Text = "Command" }, row, 0);
			AddToGrid(grid, new TextBlock() { Text = outputValue, Foreground = outputNormal ? Brushes.Black : Brushes.Red, FontWeight = outputNormal ? FontWeights.Regular : FontWeights.Bold, TextAlignment = TextAlignment.Right }, row++, 2);

			Grid.SetRowSpan(grid.Children[0], int.MaxValue);
		}
	}
}
