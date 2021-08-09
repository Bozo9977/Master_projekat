using Common.CalculationEngine;
using Common.DataModel;
using Common.SCADA;
using Common.WCF;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GUI.View
{
	class SwitchingExecutionView : View
	{
		enum EStepState { Pending, Next, Succeeded, Failed }

		class SwitchingStepInternal
		{
			public EStepState State { get; set; }
			public long Switch { get; private set; }
			public bool Open { get; private set; }
			public int Index { get; private set; }
			public long SwitchSignal { get; private set; }
			public bool Auto { get { return SwitchSignal > 0; } }

			public SwitchingStepInternal(long sw, bool open, int index, long switchSignal)
			{
				Switch = sw;
				Open = open;
				Index = index;
				State = EStepState.Pending;
				SwitchSignal = switchSignal;
			}

			public SwitchingStepInternal(SwitchingStep step, long switchSignal) : this(step.Switch, step.Open, step.Index, switchSignal)
			{ }
		}

		List<SwitchingStepInternal> steps;
		StackPanel panel;
		bool initialized;
		long gid;
		PubSubClient pubSub;
		int nextStep = 0;

		public override UIElement Element
		{
			get
			{
				if(!initialized)
					Update();

				return panel;
			}
		}

		public override string Title { get { return "SwitchingSchedule " + gid + " execution"; } }

		public SwitchingExecutionView(long gid, PubSubClient pubSub) : base()
		{
			this.gid = gid;
			this.pubSub = pubSub;
			panel = new StackPanel();

			SwitchingSchedule ss = pubSub.Model.Get(gid) as SwitchingSchedule;
			steps = GetSteps(ss);
		}

		public override void Update(EObservableMessageType msg)
		{
			if(!initialized || msg == EObservableMessageType.NetworkModelChanged)
				Update();
		}

		public override void Update()
		{
			if(!initialized)
			{
				panel.Children.Add(CreateHeading());
				panel.Children.Add(CreateStepsPanel());
				initialized = true;
			}

			UpdateStepsPanel(panel.Children[1] as StackPanel, steps);
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

				steps.Add(new SwitchingStepInternal(step, pubSub.Model.GetSwitchSignal(step.Switch)));
			}

			if(steps.Count > 0)
				steps[0].State = EStepState.Next;

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

			AddToGrid(grid, new TextBlock() { Text = "State", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 0);
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

				AddToGrid(stepsGrid, CreateStateCell(step), row, 0);
				AddToGrid(stepsGrid, new TextBlock() { Text = step.Index.ToString(), VerticalAlignment = VerticalAlignment.Center }, row, 2);

				TextBlock swLink = CreateHyperlink(step.Switch.ToString(), () => new ElementWindow(step.Switch, pubSub).Show());
				swLink.VerticalAlignment = VerticalAlignment.Center;

				AddToGrid(stepsGrid, swLink, row, 4);
				AddToGrid(stepsGrid, new TextBlock() { Text = step.Open ? "Open" : "Close", VerticalAlignment = VerticalAlignment.Center }, row, 6);
				AddToGrid(stepsGrid, new TextBlock() { Text = step.Auto.ToString(), VerticalAlignment = VerticalAlignment.Center }, row, 8);
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

		UIElement CreateStateCell(SwitchingStepInternal step)
		{
			if(step.State != EStepState.Next)
				return new TextBlock() { Text = step.State.ToString(), VerticalAlignment = VerticalAlignment.Center, HorizontalAlignment = HorizontalAlignment.Center };

			Button btn = new Button() { Content = step.Auto ? "Command" : "Confirm" };
			btn.Click += (x, y) => ExecuteStep();

			return btn;
		}

		void ExecuteStep()
		{
			if(nextStep < 0 || nextStep >= steps.Count)
				return;

			SwitchingStepInternal step = steps[nextStep];
			bool success = false;

			if(step.Auto)
			{
				Client<ISCADAServiceContract> clientSCADA = new Client<ISCADAServiceContract>("endpointSCADA");
				clientSCADA.Connect();
				clientSCADA.Call<bool>(scada => { scada.CommandDiscrete(new List<long>() { step.SwitchSignal }, new List<int>() { step.Open ? 1 : 0 }); return true; }, out success);
				clientSCADA.Disconnect();
			}
			else
			{
				Client<ICalculationEngineServiceContract> clientCE = new Client<ICalculationEngineServiceContract>("endpointCE");
				clientCE.Connect();
				clientCE.Call<bool>(ce => ce.MarkSwitchState(step.Switch, step.Open), out success);
				clientCE.Disconnect();
			}

			step.State = success ? EStepState.Succeeded : EStepState.Failed;

			++nextStep;

			if(nextStep < steps.Count)
			{
				steps[nextStep].State = EStepState.Next;
			}

			UpdateStepsPanel(panel.Children[1] as StackPanel, steps);
		}
	}
}
