using Common.CalculationEngine;
using Common.DataModel;
using Common.GDA;
using Common.WCF;
using System;
using System.Windows;
using System.Windows.Controls;

namespace GUI.View
{
	public class SwitchStateView : View
	{
		PubSubClient pubSub;
		Func<Switch> switchGetter;
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

		public SwitchStateView(Func<Switch> switchGetter, PubSubClient pubSub) : base()
		{
			this.switchGetter = switchGetter;
			this.pubSub = pubSub;
			panel = new StackPanel();
		}

		void MarkSwitch(bool open)
		{
			Switch s = switchGetter();

			if(s == null)
				return;

			Client<ICalculationEngineServiceContract> client = new Client<ICalculationEngineServiceContract>("endpointCE");
			client.Connect();
			client.Call<bool>(ce => { ce.MarkSwitchState(s.GID, open); return true; }, out _);
			client.Disconnect();
		}

		void UnmarkSwitch()
		{
			Switch s = switchGetter();

			if(s == null)
				return;

			Client<ICalculationEngineServiceContract> client = new Client<ICalculationEngineServiceContract>("endpointCE");
			client.Connect();
			client.Call<bool>(ce => { ce.UnmarkSwitchState(s.GID); return true; }, out _);
			client.Disconnect();
		}

		public override void Update(EObservableMessageType msg)
		{
			if(msg != EObservableMessageType.NetworkModelChanged && msg != EObservableMessageType.SwitchStatusChanged && msg != EObservableMessageType.MarkedSwitchesChanged)
				return;

			Update();
		}

		public override void Update()
		{
			Switch io = switchGetter();

			if(io == null)
			{
				panel.Children.Clear();
				return;
			}

			bool hasSCADA = false;
			bool marked = false;
			bool defaulted = false;
			bool open = false;

			for(int i = 0; i < io.Measurements.Count; ++i)
			{
				long gid = io.Measurements[i];

				if(ModelCodeHelper.GetTypeFromGID(gid) != DMSType.Discrete)
					continue;

				Discrete d = pubSub.Model.Get(gid) as Discrete;

				if(d == null || d.MeasurementType != MeasurementType.SwitchState)
					continue;

				int state;
				if(pubSub.Measurements.GetDiscreteInput(gid, out state))
				{
					hasSCADA = true;
					open = state != 0;
					break;
				}
			}

			if(!hasSCADA && !(marked = pubSub.Topology.TryGetMarkedSwitch(io.GID, out open)))
			{
				open = io.NormalOpen;
				defaulted = true;
			}

			TextBlock tbState = new TextBlock() { Margin = new Thickness(2), Text = (open ? "Open" : "Closed") + (hasSCADA ? " (Measured)" : (marked ? " (Marked)" : " (Default)")), VerticalAlignment = VerticalAlignment.Center };

			Button markOpenButton = new Button() { Margin = new Thickness(2), Content = "Mark as open", IsEnabled = defaulted || (marked && !open), VerticalAlignment = VerticalAlignment.Center };
			markOpenButton.Click += (x, y) => MarkSwitch(true);
			
			Button markClosedButton = new Button() { Margin = new Thickness(2), Content = "Mark as closed", IsEnabled = defaulted || (marked && open), VerticalAlignment = VerticalAlignment.Center };
			markClosedButton.Click += (x, y) => MarkSwitch(false);

			Button unmarkButton = new Button() { Margin = new Thickness(2), Content = "Unmark", IsEnabled = marked, VerticalAlignment = VerticalAlignment.Center };
			unmarkButton.Click += (x, y) => UnmarkSwitch();

			StackPanel sp = new StackPanel() { Margin = new Thickness(1, 0, 1, 0), Orientation = Orientation.Horizontal };
			sp.Children.Add(tbState);
			sp.Children.Add(markOpenButton);
			sp.Children.Add(markClosedButton);
			sp.Children.Add(unmarkButton);

			panel.Children.Clear();
			panel.Children.Add(new TextBlock() { Margin = new Thickness(2), Text = "Switch state", FontWeight = FontWeights.Bold, FontSize = 14 });
			panel.Children.Add(sp);

			initialized = true;
		}
	}
}