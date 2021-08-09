using Common.DataModel;
using Common.GDA;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace GUI.View
{
	public abstract class ElementView : View
	{
		protected long GID { get; private set; }
		protected PubSubClient PubSub { get; private set; }

		public ElementView(long gid, PubSubClient pubSub) : base()
		{
			GID = gid;
			PubSub = pubSub;
		}
	}

	public class MaybeElementView : ElementView
	{
		StackPanel panel;
		ElementView elementView;
		NullView nullView;
		bool isNull;
		Dictionary<DMSType, ModelCode> dmsTypeToModelCodeMap;
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

		public MaybeElementView(long gid, PubSubClient pubSub) : base(gid, pubSub)
		{
			dmsTypeToModelCodeMap = ModelResourcesDesc.GetTypeToModelCodeMap();
			elementView = InitView();
			nullView = new NullView();
			panel = new StackPanel();
		}

		ElementView InitView()
		{
			DMSType type = ModelCodeHelper.GetTypeFromGID(GID);
			ModelCode mc;
			ElementView v;

			if(!dmsTypeToModelCodeMap.TryGetValue(type, out mc))
				return null;

			if(ModelCodeHelper.ModelCodeClassIsSubClassOf(mc, ModelCode.SWITCH))
			{
				v = new SwitchView(GID, PubSub);
			}
			else if(ModelCodeHelper.ModelCodeClassIsSubClassOf(mc, ModelCode.POWERSYSTEMRESOURCE))
			{
				v = new PowerSystemResourceView(GID, PubSub);
			}
			else if(type == DMSType.ConnectivityNode)
			{
				v = new ConnectivityNodeView(GID, PubSub);
			}
			else if(type == DMSType.Discrete || type == DMSType.Analog)
			{
				v = new MeasurementView(GID, PubSub);
			}
			else if(type == DMSType.SwitchingSchedule)
			{
				v = new SwitchingScheduleView(GID, PubSub);
			}
			else
			{
				v = new IdentifiedObjectView(GID, PubSub);
			}

			return v;
		}

		public override void Update(EObservableMessageType msg)
		{
			if(!initialized)
			{
				Update();
				return;
			}

			bool replace = msg == EObservableMessageType.NetworkModelChanged && RefreshNull();
			View child = isNull ? (View)nullView : (View)elementView;
			child.Update(msg);

			if(replace)
			{
				panel.Children.Clear();
				panel.Children.Add(child.Element);
			}
		}
		
		private bool RefreshNull()
		{
			bool newIsNull = PubSub.Model.Get(GID) == null;

			if(initialized && newIsNull == isNull)
				return false;

			isNull = newIsNull;
			return true;
		}

		public override void Update()
		{
			bool replace = RefreshNull();
			View child = isNull ? (View)nullView : (View)elementView;
			child.Update();

			if(replace)
			{
				panel.Children.Clear();
				panel.Children.Add(child.Element);
			}

			initialized = true;
		}
	}

	public class IdentifiedObjectView : ElementView
	{
		protected PropertiesView properties;
		IdentifiedObject io;
		bool initialized;
		StackPanel panel;

		public override UIElement Element
		{
			get
			{
				if(!initialized)
					Update();

				return panel;
			}
		}

		public IdentifiedObjectView(long gid, PubSubClient pubSub) : base(gid, pubSub)
		{
			properties = new PropertiesView(() => io, pubSub);
			panel = new StackPanel();
		}

		public override void Update(EObservableMessageType msg)
		{
			if(!initialized || msg == EObservableMessageType.NetworkModelChanged)
				io = PubSub.Model.Get(GID);
			
			properties.Update(msg);

			if(!initialized)
			{
				panel.Children.Add(properties.Element);
				initialized = true;
			}
		}

		public override void Update()
		{
			io = PubSub.Model.Get(GID);
			properties.Update();

			if(!initialized)
			{
				panel.Children.Add(properties.Element);
				initialized = true;
			}
		}
	}
}
