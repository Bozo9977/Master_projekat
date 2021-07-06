using Common.DataModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GUI.View
{
	public class SwitchView : ElementView
	{
		PropertiesView properties;
		MeasurementsView measurements;
		SwitchStateView switchState;
		Switch io;
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

		public SwitchView(long gid, PubSubClient pubSub) : base(gid, pubSub)
		{
			properties = new PropertiesView(() => io, pubSub);
			measurements = new MeasurementsView(() => io == null ? (IEnumerable<long>)new long[0] : io.Measurements, pubSub);
			switchState = new SwitchStateView(() => io, pubSub);
			panel = new StackPanel();
		}

		public override void Update(EObservableMessageType msg)
		{
			if(!initialized || msg == EObservableMessageType.NetworkModelChanged)
				io = PubSub.Model.Get(GID) as Switch;

			properties.Update(msg);
			measurements.Update(msg);
			switchState.Update(msg);

			if(!initialized)
			{
				panel.Children.Add(properties.Element);
				panel.Children.Add(measurements.Element);
				initialized = true;
			}
		}

		public override void Update()
		{
			io = PubSub.Model.Get(GID) as Switch;

			properties.Update();
			measurements.Update();
			switchState.Update();

			if(!initialized)
			{
				panel.Children.Add(properties.Element);
				panel.Children.Add(measurements.Element);
				panel.Children.Add(switchState.Element);
				initialized = true;
			}
		}
	}
}
