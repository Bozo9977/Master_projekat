using Common.GDA;
using GUI.View;
using System;
using System.Windows;

namespace GUI
{
	/// <summary>
	/// Interaction logic for ElementsWindow.xaml
	/// </summary>
	public partial class ElementsWindow : Window, Common.IObserver<ObservableMessage>
	{
		public DMSType Type { get; private set; }
		PubSubClient pubSub;
		View.View view;

		public ElementsWindow(DMSType type, PubSubClient pubSub)
		{
			Type = type;
			this.pubSub = pubSub;
			pubSub.Subscribe(this);

			InitializeComponent();
			Title = Type.ToString() + "s";
			view = GetView(type, pubSub);

			if(view == null)
				return;

			panel.Children.Add(view.Element);
		}

		private View.View GetView(DMSType type, PubSubClient pubSub)
		{
			if(type == DMSType.SwitchingSchedule)
				return new SwitchingSchedulesView(pubSub);

			return null;
		}

		public void Notify(ObservableMessage message)
		{
			Dispatcher.BeginInvoke(new Action<EObservableMessageType>(view.Update), message.Type);
		}

		public new void Show()
		{
			foreach(Window w in Application.Current.MainWindow.OwnedWindows)
			{
				ElementsWindow ew = w as ElementsWindow;

				if(ew != null && ew.Type == Type)
				{
					ew.Focus();
					return;
				}
			}

			Owner = Application.Current.MainWindow;
			base.Show();
		}
	}
}
