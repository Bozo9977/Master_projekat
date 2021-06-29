using Common.DataModel;
using System.Collections.Generic;

namespace GUI.View
{
	public class ConnectivityNodeView : ElementView
	{
		PropertiesView properties;
		MeasurementsView measurements;

		public ConnectivityNodeView(ConnectivityNode io, PubSubClient pubSub) : base(io, pubSub)
		{
			properties = new PropertiesView(io, pubSub);
			measurements = new MeasurementsView(io, x => GetMeasurements((ConnectivityNode)x), pubSub);
		}

		public override void Refresh()
		{
			properties.Refresh();
			measurements.Refresh();

			Panel.Children.Clear();
			Panel.Children.Add(properties.Panel);
			Panel.Children.Add(measurements.Panel);
		}

		IEnumerable<long> GetMeasurements(ConnectivityNode io)
		{
			List<long> measurements = new List<long>();
			NetworkModel nm = PubSub.Model;

			foreach(long terminalGID in io.Terminals)
			{
				Terminal terminal = (Terminal)nm.Get(terminalGID);

				if(terminal == null)
					continue;

				foreach(long measGID in terminal.Measurements)
				{
					measurements.Add(measGID);
				}
			}

			return measurements;
		}
	}
}
