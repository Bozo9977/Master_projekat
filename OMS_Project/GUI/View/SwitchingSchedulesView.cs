using Common.DataModel;
using Common.GDA;
using Common.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GUI.View
{
	class SwitchingSchedulesView : View
	{
		PubSubClient pubSub;
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

		public SwitchingSchedulesView(PubSubClient pubSub) : base()
		{
			this.pubSub = pubSub;
			panel = new StackPanel();

			StackPanel listPanel = new StackPanel();
			Border border = new Border() { BorderThickness = new Thickness(1), BorderBrush = Brushes.LightGray, Margin = new Thickness(1), Padding = new Thickness(1) };
			Grid listGrid = new Grid();
			listGrid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			listGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(5) });
			listGrid.ColumnDefinitions.Add(new ColumnDefinition() { MinWidth = 0 });
			listGrid.RowDefinitions.Add(new RowDefinition());

			AddToGrid(listGrid, new TextBlock() { Text = "GID", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 0);
			AddToGrid(listGrid, new TextBlock() { Text = "Name", FontWeight = FontWeights.Bold, HorizontalAlignment = HorizontalAlignment.Center }, 0, 2);

			AddToGrid(listGrid, new GridSplitter() { ResizeDirection = GridResizeDirection.Columns, HorizontalAlignment = HorizontalAlignment.Stretch, Background = Brushes.Black, Margin = new Thickness(0), Padding = new Thickness(0), ResizeBehavior = GridResizeBehavior.PreviousAndNext, BorderThickness = new Thickness(2, 0, 2, 0), BorderBrush = Brushes.Transparent }, 0, 1);

			border.Child = listGrid;
			listPanel.Children.Add(border);

			StackPanel btnPanel = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(2) };
			
			Button btn = new Button() { Content = "Create new", Margin = new Thickness(2) };
			btn.Click += (x, y) => CreateSwitchingSchedule();

			btnPanel.Children.Add(btn);

			panel.Children.Add(listPanel);
			panel.Children.Add(btnPanel);
		}

		void CreateSwitchingSchedule()
		{
			long id = ModelCodeHelper.CreateGID(0, DMSType.SwitchingSchedule, -1);
			string name = "SwitchingSchedule_" + DateTime.UtcNow.ToString("o");

			ResourceDescription rd = new ResourceDescription(id);
			rd.AddProperty(new StringProperty(ModelCode.IDENTIFIEDOBJECT_MRID, name));
			rd.AddProperty(new StringProperty(ModelCode.IDENTIFIEDOBJECT_NAME, name));

			Delta delta = new Delta();
			delta.InsertOperations.Add(rd);

			Client<INetworkModelGDAContract> clientNMS = new Client<INetworkModelGDAContract>("endpointNMS");
			clientNMS.Connect();

			UpdateResult result;
			long gid;

			if(!clientNMS.Call<UpdateResult>(nms => nms.ApplyUpdate(delta), out result) || result == null || !result.Inserted.TryGetValue(id, out gid))
			{
				clientNMS.Disconnect();
				return;
			}

			clientNMS.Disconnect();

			new ElementWindow(gid, pubSub).Show();
		}

		public override void Update(EObservableMessageType msg)
		{
			if(initialized && msg != EObservableMessageType.NetworkModelChanged)
				return;

			Update();
		}

		public override void Update()
		{
			Grid grid = (Grid)((Border)((StackPanel)panel.Children[0]).Children[0]).Child;

			if(grid.Children.Count > 3)
				grid.Children.RemoveRange(3, grid.Children.Count - 3);

			if(grid.RowDefinitions.Count > 1)
				grid.RowDefinitions.RemoveRange(1, grid.RowDefinitions.Count - 1);

			IEnumerable<long> gids = pubSub.Model.GetGIDsByType(DMSType.SwitchingSchedule);

			int i = 0;

			foreach(long gid in gids)
			{
				SwitchingSchedule ss = pubSub.Model.Get(gid) as SwitchingSchedule;

				if(ss == null)
					continue;

				grid.RowDefinitions.Add(new RowDefinition());
				AddToGrid(grid, CreateHyperlink(gid.ToString(), () => new ElementWindow(gid, pubSub).Show()), i + 1, 0);
				AddToGrid(grid, new TextBlock() { Text = ss.Name.ToString() }, i + 1, 2);

				++i;
			}

			if(i == 0)
			{
				grid.RowDefinitions.Add(new RowDefinition());
				Grid.SetColumnSpan(AddToGrid(grid, new TextBlock() { Text = "Empty", HorizontalAlignment = HorizontalAlignment.Center  }, 1, 0), int.MaxValue);
				Grid.SetRowSpan(grid.Children[2], 1);
			}
			else
			{
				Grid.SetRowSpan(grid.Children[2], int.MaxValue);
			}

			initialized = true;
		}

		void DeleteSwitchingSchedule(SwitchingSchedule ss)
		{
			Delta delta = new Delta();
			delta.DeleteOperations.Add(new ResourceDescription(ss.GID));

			foreach(long gid in ss.SwitchingSteps)
			{
				delta.DeleteOperations.Add(new ResourceDescription(gid));
			}

			Client<INetworkModelGDAContract> clientNMS = new Client<INetworkModelGDAContract>("endpointNMS");
			clientNMS.Connect();

			UpdateResult result;

			clientNMS.Call<UpdateResult>(nms => nms.ApplyUpdate(delta), out result);

			clientNMS.Disconnect();
			return;
		}
	}
}
