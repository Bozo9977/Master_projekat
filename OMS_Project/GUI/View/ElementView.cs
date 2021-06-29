using Common.DataModel;

namespace GUI.View
{
	public abstract class ElementView : View
	{
		protected IdentifiedObject IO { get; private set; }
		protected PubSubClient PubSub { get; private set; }

		public ElementView(IdentifiedObject io, PubSubClient pubSub) : base()
		{
			IO = io;
			PubSub = pubSub;
		}
	}

	public class OtherView : ElementView
	{
		protected PropertiesView properties;

		public OtherView(IdentifiedObject io, PubSubClient pubSub) : base(io, pubSub)
		{
			properties = new PropertiesView(io, pubSub);
		}

		public override void Refresh()
		{
			properties.Refresh();

			Panel.Children.Clear();
			Panel.Children.Add(properties.Panel);
		}
	}
}
