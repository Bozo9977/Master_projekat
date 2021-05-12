using Common.DataModel;
using Common.GDA;
using System;
using System.Windows;
using System.Windows.Controls;
using GUI.View;

namespace GUI
{
	public partial class ElementWindow : Window, Common.IObserver<ObservableMessage>
	{
		public long GID { get; private set; }
		PubSubClient pubSub;
		ElementView viewModel;

		public ElementWindow(long gid, PubSubClient pubSub)
		{
			InitializeComponent();
			GID = gid;
			this.pubSub = pubSub;
			pubSub.Subscribe(this);
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
				InitView();

			viewModel.Refresh(io);
		}

		void InitView()
		{
			DMSType type = ModelCodeHelper.GetTypeFromGID(GID);
			ModelCode mc = ModelCodeHelper.GetModelCodeByType(type);
			ElementView vm;

			if(ModelCodeHelper.ModelCodeClassIsSubClassOf(mc, ModelCode.CONDUCTINGEQUIPMENT))
			{
				vm = new ConductingEquipmentView(panel, pubSub);
			}
			else if(type == DMSType.ConnectivityNode)
			{
				vm = new ConnectivityNodeView(panel, pubSub);
			}
			else if(type == DMSType.Discrete || type == DMSType.Analog)
			{
				vm = new MeasurementView(panel, pubSub);
			}
			else
			{
				vm = new ElementView(panel, pubSub);
			}

			viewModel = vm;
		}
	}
}
