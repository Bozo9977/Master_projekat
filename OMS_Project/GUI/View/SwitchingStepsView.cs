using Common.DataModel;
using Common.GDA;
using Common.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GUI.View
{
	class SwitchingStepsView : View
	{
		class SwitchingStepInternal
		{
			public long SwitchingSchedule { get; private set; }
			public long Switch { get; private set; }
			public bool Open { get; private set; }
			public int Index { get; set; }
			public string Auto { get; private set; }
			public CheckBox CheckBox { get; private set; }

			public SwitchingStepInternal(long switchingSchedule, long sw, bool open, int index, string auto, CheckBox checkBox)
			{
				SwitchingSchedule = switchingSchedule;
				Switch = sw;
				Open = open;
				Index = index;
				Auto = auto;
				CheckBox = checkBox;
			}

			public SwitchingStepInternal(SwitchingStep step, string auto, CheckBox checkBox) : this(step.SwitchingSchedule, step.Switch, step.Open, step.Index, auto, checkBox)
			{ }
		}

		List<SwitchingStepInternal> steps;
		StackPanel panel;
		bool initialized;
		long gid;
		PubSubClient pubSub;

		public override UIElement Element
		{
			get
			{
				if(!initialized)
					Update();

				return panel;
			}
		}

		public SwitchingStepsView(long gid, PubSubClient pubSub) : base()
		{
			this.gid = gid;
			this.pubSub = pubSub;
			panel = new StackPanel();
		}

		public override void Update(EObservableMessageType msg)
		{
			if(!initialized || msg == EObservableMessageType.NetworkModelChanged)
				Update();
		}

		public override void Update()
		{
			SwitchingSchedule ss = pubSub.Model.Get(gid) as SwitchingSchedule;

			steps = GetSteps(ss);

			if(!initialized)
			{
				panel.Children.Add(CreateHeading());
				panel.Children.Add(CreateStepsPanel());
				//panel.Children.Add(CreateButtonsPanel());
				initialized = true;
			}

			UpdateStepsPanel(panel.Children[1] as StackPanel, steps);
		}

		string GetSwitchAutomatic(long gid)
		{
			Switch sw = pubSub.Model.Get(gid) as Switch;

			if(sw == null)
				return "N/A";

			bool hasSCADA = false;

			for(int j = 0; j < sw.Measurements.Count; ++j)
			{
				long measGID = sw.Measurements[j];

				if(ModelCodeHelper.GetTypeFromGID(measGID) != DMSType.Discrete)
					continue;

				Discrete d = pubSub.Model.Get(measGID) as Discrete;

				if(d == null || d.MeasurementType != MeasurementType.SwitchState)
					continue;

				hasSCADA = true;
				break;
			}

			return hasSCADA.ToString();
		}

		List<SwitchingStepInternal> GetSteps(SwitchingSchedule ss)
		{
			if(ss == null)
				return null;

			List<SwitchingStepInternal> steps = new List<SwitchingStepInternal>(ss.SwitchingSteps.Count);

			for(int i = 0; i < ss.SwitchingSteps.Count; ++i)
			{
				SwitchingStep step = pubSub.Model.Get(ss.SwitchingSteps[i]) as SwitchingStep;

				if(step == null)
					continue;

				steps.Add(new SwitchingStepInternal(step, GetSwitchAutomatic(step.Switch), new CheckBox()));
			}

			return steps;
		}

		UIElement CreateHeading()
		{
			return new TextBlock() { Margin = new Thickness(2), Text = "Switching steps", FontWeight = FontWeights.Bold, FontSize = 14 };
		}

		StackPanel CreateStepsPanel()
		{
			StackPanel stepsPanel = new StackPanel();
			Border border = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.LightGray, Margin = new Thickness(1), Padding = new Thickness(1) };
			Grid grid = new Grid();
			grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
			grid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			grid.RowDefinitions.Add(new RowDefinition());

			AddToGrid(grid, new TextBlock() { Text = "Selected", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 0);
			AddToGrid(grid, new TextBlock() { Text = "Index", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 2);
			AddToGrid(grid, new TextBlock() { Text = "Switch", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 4);
			AddToGrid(grid, new TextBlock() { Text = "Action", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 6);
			AddToGrid(grid, new TextBlock() { Text = "Auto", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 8);

			AddToGrid(grid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 1);
			AddToGrid(grid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 3);
			AddToGrid(grid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 5);
			AddToGrid(grid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 7);

			border.Child = grid;
			stepsPanel.Children.Add(border);

			return stepsPanel;
		}

		public bool Save()
		{
			Delta delta = new Delta();

			for(int i = 0; i < steps.Count; ++i)
			{
				SwitchingStepInternal step = steps[i];
				ResourceDescription rd = new ResourceDescription(ModelCodeHelper.CreateGID(0, DMSType.SwitchingStep, -(i + 1)));
				string name = "SwitchingStep_" + gid + "_" + i;

				rd.AddProperty(new StringProperty(ModelCode.IDENTIFIEDOBJECT_MRID, name));
				rd.AddProperty(new StringProperty(ModelCode.IDENTIFIEDOBJECT_NAME, name));
				rd.AddProperty(new ReferenceProperty(ModelCode.SWITCHINGSTEP_SWITCHINGSCHEDULE, gid));
				rd.AddProperty(new ReferenceProperty(ModelCode.SWITCHINGSTEP_SWITCH, step.Switch));
				rd.AddProperty(new BoolProperty(ModelCode.SWITCHINGSTEP_OPEN, step.Open));
				rd.AddProperty(new Int32Property(ModelCode.SWITCHINGSTEP_INDEX, i));

				delta.InsertOperations.Add(rd);
			}

			Client<INetworkModelGDAContract> clientNMS = new Client<INetworkModelGDAContract>("endpointNMS");
			clientNMS.Connect();

			UpdateResult result;

			clientNMS.Call<UpdateResult>(
			nms => 
			{
				int iterator = nms.GetRelatedValues(gid, new List<ModelCode>(), new Association(ModelCode.SWITCHINGSCHEDULE_SWITCHINGSTEPS, ModelCode.SWITCHINGSTEP, false), false);
				
				if(iterator < 0)
					return null;

				List<ResourceDescription> rds;

				do
				{
					rds = nms.IteratorNext(1024, iterator, false);

					if(rds == null)
						break;

					delta.DeleteOperations.AddRange(rds);
				}
				while(rds.Count >= 1024);

				return nms.ApplyUpdate(delta);
			}, out result);

			clientNMS.Disconnect();
			
			if(result == null || result.Result != ResultType.Success)
			{
				return false;
			}

			Update();
			return true;
		}

		public bool Add(int idx, long swGID, bool o)
		{
			if(steps == null)
				return false;

			if(idx > steps.Count)
				idx = steps.Count;
			else if (idx < 0)
				idx = 0;

			steps.Insert(idx, new SwitchingStepInternal(gid, swGID, o, idx, GetSwitchAutomatic(swGID), new CheckBox()));

			for(int i = idx + 1; i < steps.Count; ++i)
			{
				++steps[i].Index;
			}

			UpdateStepsPanel(panel.Children[1] as StackPanel, steps);
			return true;
		}

		public void DeleteSelected()
		{
			for(int i = 0; i < steps.Count; ++i)
			{
				SwitchingStepInternal step = steps[i];

				if(step.CheckBox.IsChecked == true)
				{
					steps.RemoveAt(i);
					--i;
				}
			}

			UpdateStepsPanel(panel.Children[1] as StackPanel, steps);
		}

		void UpdateStepsPanel(StackPanel stepsPanel, IEnumerable<SwitchingStepInternal> steps)
		{
			Grid stepsGrid = (Grid)((Border)stepsPanel.Children[0]).Child;

			if(stepsGrid.Children.Count > 9)
				stepsGrid.Children.RemoveRange(9, stepsGrid.Children.Count - 9);

			if(stepsGrid.RowDefinitions.Count > 1)
				stepsGrid.RowDefinitions.RemoveRange(1, stepsGrid.RowDefinitions.Count - 1);

			int row = 0;

			if(steps == null)
				steps = new SwitchingStepInternal[0];

			foreach(SwitchingStepInternal step in steps)
			{
				++row;

				if(step == null)
					continue;

				stepsGrid.RowDefinitions.Add(new RowDefinition());

				AddToGrid(stepsGrid, step.CheckBox, row, 0);
				AddToGrid(stepsGrid, new TextBlock() { Text = step.Index.ToString() }, row, 2);
				AddToGrid(stepsGrid, CreateHyperlink(step.Switch.ToString(), () => new ElementWindow(step.Switch, pubSub).Show()), row, 4);
				AddToGrid(stepsGrid, new TextBlock() { Text = step.Open ? "Open" : "Close" }, row, 6);
				AddToGrid(stepsGrid, new TextBlock() { Text = step.Auto }, row, 8);
			}

			int splitterRowSpan = int.MaxValue;

			if(row == 0)
			{
				stepsGrid.RowDefinitions.Add(new RowDefinition());
				Grid.SetColumnSpan(AddToGrid(stepsGrid, new TextBlock() { Text = "Empty", HorizontalAlignment = HorizontalAlignment.Center }, 1, 0), int.MaxValue);
				splitterRowSpan = 1;
			}

			Grid.SetRowSpan(stepsGrid.Children[5], splitterRowSpan);
			Grid.SetRowSpan(stepsGrid.Children[6], splitterRowSpan);
			Grid.SetRowSpan(stepsGrid.Children[7], splitterRowSpan);
			Grid.SetRowSpan(stepsGrid.Children[8], splitterRowSpan);

			stepsGrid.RowDefinitions.Last().Height = new GridLength(1, GridUnitType.Star);
		}
	}
}
