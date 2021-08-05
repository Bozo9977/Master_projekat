using Common.GDA;
using System;
using System.Windows;
using GUI.View;

namespace GUI
{
	public partial class ElementWindow : Window, Common.IObserver<ObservableMessage>
	{
		public long GID { get; private set; }
		PubSubClient pubSub;
		View.View view;

		public ElementWindow(long gid, PubSubClient pubSub)
		{
			GID = gid;
			this.pubSub = pubSub;
			pubSub.Subscribe(this);

			InitializeComponent();
			Title = ModelCodeHelper.GetTypeFromGID(gid) + " " + gid;
			view = new MaybeElementView(gid, pubSub);
			panel.Children.Add(view.Element);
		}

		public void Notify(ObservableMessage message)
		{
			Dispatcher.BeginInvoke(new Action<EObservableMessageType>(view.Update), message.Type);
		}

		public new void Show()
		{
			foreach(Window w in Application.Current.MainWindow.OwnedWindows)
			{
				ElementWindow ew = w as ElementWindow;

				if(ew != null && ew.GID == GID)
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
