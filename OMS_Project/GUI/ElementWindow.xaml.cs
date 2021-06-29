using Common.DataModel;
using Common.GDA;
using System;
using System.Windows;
using System.Windows.Controls;
using GUI.View;
using System.Collections.Generic;

namespace GUI
{
	public partial class ElementWindow : Window, Common.IObserver<ObservableMessage>
	{
		public long GID { get; private set; }
		PubSubClient pubSub;
		ElementView viewModel;
		Dictionary<DMSType, ModelCode> dmsTypeToModelCodeMap;

		public ElementWindow(long gid, PubSubClient pubSub)
		{
			GID = gid;
			this.pubSub = pubSub;
			pubSub.Subscribe(this);
			dmsTypeToModelCodeMap = ModelResourcesDesc.GetTypeToModelCodeMap();

			InitializeComponent();
			RefreshInternal();
		}

		public void Notify(ObservableMessage message)
		{
			switch(message.Type)
			{
				case EObservableMessageType.NetworkModelChanged:
				case EObservableMessageType.MeasurementValuesChanged:
					Refresh();
					break;
			}
		}

		void Refresh()
		{
			Dispatcher.BeginInvoke(new Action(RefreshInternal));
		}

		void RefreshInternal()
		{
			NetworkModel nm = pubSub.Model;
			IdentifiedObject io = nm.Get(GID);

			if(io == null)
			{
				panel.Children.Clear();
				Title = "Element " + GID;
				panel.Children.Add(new TextBlock() { Margin = new Thickness(5), Text = "The element does not exist.", HorizontalAlignment = HorizontalAlignment.Center });
				viewModel = null;
				return;
			}

			Title = ModelCodeHelper.GetTypeFromGID(GID) + " " + GID;

			if(viewModel == null)
				viewModel = InitView(io);

			viewModel.Refresh();

			panel.Children.Clear();
			panel.Children.Add(viewModel.Panel);
		}

		ElementView InitView(IdentifiedObject io)
		{
			DMSType type = ModelCodeHelper.GetTypeFromGID(GID);
			ModelCode mc = dmsTypeToModelCodeMap[type];
			ElementView vm;

			if(ModelCodeHelper.ModelCodeClassIsSubClassOf(mc, ModelCode.CONDUCTINGEQUIPMENT))
			{
				vm = new ConductingEquipmentView((ConductingEquipment)io, pubSub);
			}
			else if(type == DMSType.ConnectivityNode)
			{
				vm = new ConnectivityNodeView((ConnectivityNode)io, pubSub);
			}
			else if(type == DMSType.Discrete || type == DMSType.Analog)
			{
				vm = new MeasurementView((Measurement)io, pubSub);
			}
			else
			{
				vm = new OtherView(io, pubSub);
			}

			return vm;
		}
	}
}
