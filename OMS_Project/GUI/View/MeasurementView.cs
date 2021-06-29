using Common.DataModel;

namespace GUI.View
{
	public class MeasurementView : ElementView
	{
		PropertiesView properties;
		CommandView command;

		public MeasurementView(Measurement io, PubSubClient pubSub) : base(io, pubSub)
		{
			properties = new PropertiesView(io, pubSub);
			command = new CommandView(io, pubSub);
		}

		public override void Refresh()
		{
			properties.Refresh();
			command.Refresh();

			Panel.Children.Clear();
			Panel.Children.Add(properties.Panel);
			Panel.Children.Add(command.Panel);
		}
	}
}
