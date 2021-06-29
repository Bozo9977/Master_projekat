using Common.DataModel;

namespace GUI.View
{
	public class ConductingEquipmentView : ElementView
	{
		PropertiesView properties;
		MeasurementsView measurements;

		public ConductingEquipmentView(ConductingEquipment io, PubSubClient pubSub) : base(io, pubSub)
		{
			properties = new PropertiesView(io, pubSub);
			measurements = new MeasurementsView(io, x => ((ConductingEquipment)x).Measurements, pubSub);
		}

		public override void Refresh()
		{
			properties.Refresh();
			measurements.Refresh();

			Panel.Children.Clear();
			Panel.Children.Add(properties.Panel);
			Panel.Children.Add(measurements.Panel);
		}
	}
}