using Common;
using Common.CalculationEngine;
using Common.DataModel;
using Common.GDA;
using Common.SCADA;
using Common.WCF;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace GUI.View
{
	public class SwitchingScheduleView : ElementView
	{
		PropertiesView properties;
		SwitchingStepsView steps;
		SwitchingSchedule io;
		StackPanel panel;
		bool initialized;

		public override UIElement Element
		{
			get
			{
				if(!initialized)
					Update();

				return panel;
			}
		}

		public SwitchingScheduleView(long gid, PubSubClient pubSub) : base(gid, pubSub)
		{
			properties = new PropertiesView(() => io, pubSub);
			steps = new SwitchingStepsView(gid, pubSub);
			panel = new StackPanel();
		}

		public override void Update(EObservableMessageType msg)
		{
			if(!initialized || msg == EObservableMessageType.NetworkModelChanged)
				Update();
		}

		public override void Update()
		{
			io = PubSub.Model.Get(GID) as SwitchingSchedule;

			properties.Update();
			steps.Update();

			if(!initialized)
			{
				panel.Children.Add(properties.Element);
				panel.Children.Add(steps.Element);
				panel.Children.Add(CreateAddStepPanel());
				panel.Children.Add(CreateOptionsPanel(io));
				initialized = true;
			}
		}

		UIElement CreateAddStepPanel()
		{
			StackPanel panel = new StackPanel() { Orientation = Orientation.Vertical };
			Grid grid = new Grid() { Margin = new Thickness(3, 0, 3, 0) };
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.ColumnDefinitions.Add(new ColumnDefinition());
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition());
			grid.RowDefinitions.Add(new RowDefinition());

			AddToGrid(grid, new TextBlock() { Text = "Index", TextAlignment = TextAlignment.Left, VerticalAlignment = VerticalAlignment.Center }, 0, 0);

			TextBox tbIndex = new TextBox() { TextAlignment = TextAlignment.Right, VerticalContentAlignment = VerticalAlignment.Center };
			AddToGrid(grid, tbIndex, 0, 1);

			AddToGrid(grid, new TextBlock() { Text = "Switch GID", TextAlignment = TextAlignment.Left, VerticalAlignment = VerticalAlignment.Center }, 1, 0);

			TextBox tbSwitch = new TextBox() { TextAlignment = TextAlignment.Right, VerticalContentAlignment = VerticalAlignment.Center };

			AddToGrid(grid, tbSwitch, 1, 1);
			AddToGrid(grid, new TextBlock() { Text = "Action", TextAlignment = TextAlignment.Left, VerticalAlignment = VerticalAlignment.Center }, 2, 0);

			ComboBox cbAction = new ComboBox();
			cbAction.Items.Add("Open");
			cbAction.Items.Add("Close");
			cbAction.SelectedIndex = 0;
			AddToGrid(grid, cbAction, 2, 1);

			Button btnAdd = new Button() { Content = "Add", HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Center, Padding = new Thickness(4, 1, 4, 1), Margin = new Thickness(0, 2, 0, 2) };
			btnAdd.Click += (x, y) => AddSwitchingStep(tbIndex, tbSwitch, cbAction);

			AddToGrid(grid, btnAdd, 3, 0);
			
			panel.Children.Add(new TextBlock() { Margin = new Thickness(2), Text = "Add switching step", FontWeight = FontWeights.Bold, FontSize = 14 });
			panel.Children.Add(grid);

			return panel;
		}

		void AddSwitchingStep(TextBox tbIndex, TextBox tbSwitch, ComboBox cbAction)
		{
			int index;
			long switchGID;
			bool open = (cbAction.SelectedItem as string) == "Open";

			if(!int.TryParse(tbIndex.Text, out index) || !long.TryParse(tbSwitch.Text, out switchGID))
				return;

			steps.Add(index, switchGID, open);
		}

		UIElement CreateOptionsPanel(SwitchingSchedule ss)
		{
			StackPanel panel = new StackPanel() { Orientation = Orientation.Vertical };

			StackPanel btnPanel = new StackPanel() { Margin = new Thickness(1, 0, 1, 0), Orientation = Orientation.Horizontal };

			Button btnReload = new Button() { Content = "Reload", Margin = new Thickness(2), Padding = new Thickness(4, 1, 4, 1) };
			btnReload.Click += (x, y) => Update();
			btnPanel.Children.Add(btnReload);

			Button btnSave = new Button() { Content = "Save", Margin = new Thickness(2), Padding = new Thickness(4, 1, 4, 1) };
			btnSave.Click += (x, y) => SaveSwitchingSchedule();
			btnPanel.Children.Add(btnSave);

			Button btnExecute = new Button() { Content = "Execute", Margin = new Thickness(2), Padding = new Thickness(4, 1, 4, 1) };
			btnExecute.Click += (x, y) => Execute();
			btnPanel.Children.Add(btnExecute);

			Button btnDeleteSteps = new Button() { Content = "Delete selected steps", Margin = new Thickness(2), Padding = new Thickness(4, 1, 4, 1) };
			btnDeleteSteps.Click += (x, y) => DeleteSelectedSteps();
			btnPanel.Children.Add(btnDeleteSteps);

			Button btnDelete = new Button() { Content = "Delete switching schedule", Margin = new Thickness(2), Padding = new Thickness(4, 1, 4, 1) };
			btnDelete.Click += (x, y) => DeleteSwitchingSchedule();
			btnPanel.Children.Add(btnDelete);

			panel.Children.Add(new TextBlock() { Margin = new Thickness(2), Text = "Options", FontWeight = FontWeights.Bold, FontSize = 14 });
			panel.Children.Add(btnPanel);

			return panel;
		}

		void Execute()
		{
			List<Pair<SwitchingStep, long>> steps = new List<Pair<SwitchingStep, long>>(io.SwitchingSteps.Count);

			foreach(long gid in io.SwitchingSteps)
			{
				SwitchingStep step = PubSub.Model.Get(gid) as SwitchingStep;

				if(step == null)
					return;

				steps.Add(new Pair<SwitchingStep, long>(step, PubSub.Model.GetSwitchSignal(step.Switch)));
			}

			Client<ICalculationEngineServiceContract> clientCE = new Client<ICalculationEngineServiceContract>("endpointCE");
			clientCE.Connect();

			Client<ISCADAServiceContract> clientSCADA = new Client<ISCADAServiceContract>("endpointSCADA");
			clientSCADA.Connect();

			foreach(Pair<SwitchingStep, long> step in steps)
			{
				if(step.Second != 0)
				{
					if(!clientSCADA.Call<bool>(scada => { scada.CommandDiscrete(new List<long>() { step.Second }, new List<int>() { step.First.Open ? 1 : 0 }); return true; }, out _))
						break;
				}
				else
				{
					if(!clientCE.Call<bool>(ce =>  ce.MarkSwitchState(step.First.Switch, step.First.Open), out _))
						break;
				}
			}

			clientSCADA.Disconnect();
			clientCE.Disconnect();
		}

		void DeleteSelectedSteps()
		{
			steps.DeleteSelected();
		}

		void SaveSwitchingSchedule()
		{
			steps.Save();
		}

		void DeleteSwitchingSchedule()
		{
			Delta delta = new Delta();
			delta.DeleteOperations.Add(new ResourceDescription(io.GID));

			foreach(long gid in io.SwitchingSteps)
			{
				delta.DeleteOperations.Add(new ResourceDescription(gid));
			}

			Client<INetworkModelGDAContract> clientNMS = new Client<INetworkModelGDAContract>("endpointNMS");
			clientNMS.Connect();

			UpdateResult result;

			clientNMS.Call<UpdateResult>(nms => nms.ApplyUpdate(delta), out result);

			clientNMS.Disconnect();
		}
	}
}
