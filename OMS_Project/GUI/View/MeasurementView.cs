using Common.DataModel;
using System.Windows;
using System.Windows.Controls;

namespace GUI.View
{
	public class MeasurementView : ElementView
	{
		PropertiesView properties;
		CommandView command;
		Measurement io;
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

		public MeasurementView(long gid, PubSubClient pubSub) : base(gid, pubSub)
		{
			properties = new PropertiesView(() => io, pubSub);
			command = new CommandView(() => io, pubSub);
			panel = new StackPanel();
		}

		public override void Update(EObservableMessageType msg)
		{
			if(!initialized || msg == EObservableMessageType.NetworkModelChanged)
				io = PubSub.Model.Get(GID) as Measurement;

			properties.Update(msg);
			command.Update(msg);

			if(!initialized)
			{
				panel.Children.Add(properties.Element);
				panel.Children.Add(command.Element);
				initialized = true;
			}
		}

		public override void Update()
		{
			io = PubSub.Model.Get(GID) as Measurement;

			properties.Update();
			command.Update();

			if(!initialized)
			{
				panel.Children.Add(properties.Element);
				panel.Children.Add(command.Element);
				initialized = true;
			}
		}
	}
}
