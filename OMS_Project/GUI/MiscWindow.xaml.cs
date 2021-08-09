using System;
using System.Windows;

namespace GUI
{
	/// <summary>
	/// Interaction logic for MiscWindow.xaml
	/// </summary>
	public partial class MiscWindow : Window, Common.IObserver<ObservableMessage>
	{
		View.View view;
		PubSubClient pubSub;

		public MiscWindow(View.View view, PubSubClient pubSub)
		{
			this.view = view;
			this.pubSub = pubSub;
			pubSub.Subscribe(this);

			InitializeComponent();
			Title = view.Title ?? "";
			panel.Children.Add(view.Element);
		}

		public void Notify(ObservableMessage message)
		{
			Dispatcher.BeginInvoke(new Action<EObservableMessageType>(view.Update), message.Type);
		}
	}
}
