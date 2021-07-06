using Common.DataModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GUI.View
{
	public class PowerSystemResourceView : ElementView
	{
		PropertiesView properties;
		MeasurementsView measurements;
		PowerSystemResource io;
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

		public PowerSystemResourceView(long gid, PubSubClient pubSub) : base(gid, pubSub)
		{
			properties = new PropertiesView(() => io, pubSub);
			measurements = new MeasurementsView(() => io == null ? (IEnumerable<long>)new long[0] : io.Measurements, pubSub);
			panel = new StackPanel();
		}

		public override void Update(EObservableMessageType msg)
		{
			if(!initialized || msg == EObservableMessageType.NetworkModelChanged)
				io = PubSub.Model.Get(GID) as PowerSystemResource;

			properties.Update(msg);
			measurements.Update(msg);

			if(!initialized)
			{
				panel.Children.Add(properties.Element);
				panel.Children.Add(measurements.Element);
				initialized = true;
			}
		}

		public override void Update()
		{
			io = PubSub.Model.Get(GID) as PowerSystemResource;

			properties.Update();
			measurements.Update();

			if(!initialized)
			{
				panel.Children.Add(properties.Element);
				panel.Children.Add(measurements.Element);
				initialized = true;
			}
		}
	}
}