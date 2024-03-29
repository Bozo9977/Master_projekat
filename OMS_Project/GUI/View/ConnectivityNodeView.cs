﻿using Common.DataModel;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GUI.View
{
	public class ConnectivityNodeView : ElementView
	{
		PropertiesView properties;
		MeasurementsView measurements;
		ConnectivityNode io;
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

		public ConnectivityNodeView(long gid, PubSubClient pubSub) : base(gid, pubSub)
		{
			properties = new PropertiesView(() => io, pubSub);
			measurements = new MeasurementsView(GetMeasurements, pubSub);
			panel = new StackPanel();
		}

		public override void Update(EObservableMessageType msg)
		{
			if(!initialized || msg == EObservableMessageType.NetworkModelChanged)
				io = PubSub.Model.Get(GID) as ConnectivityNode;

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
			io = PubSub.Model.Get(GID) as ConnectivityNode;

			properties.Update();
			measurements.Update();

			if(!initialized)
			{
				panel.Children.Add(properties.Element);
				panel.Children.Add(measurements.Element);
				initialized = true;
			}
		}

		IEnumerable<long> GetMeasurements()
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
