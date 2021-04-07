using Common.PubSub;
using Common.WCF;
using System.Collections.Generic;
using System.Threading;

namespace GUI
{
	public enum EObservableMessageType { NetworkModelChanged }

	public class ObservableMessage
	{
		public EObservableMessageType Type { get; private set; }

		public ObservableMessage(EObservableMessageType type)
		{
			Type = type;
		}
	}

	class PubSubClient : IPubSubClient, IObservable<ObservableMessage>
	{
		NetworkModel model;
		List<IObserver<ObservableMessage>> observers;
		ReaderWriterLockSlim observersLock;
		DuplexClient<ISubscribing, IPubSubClient> client;
		ReaderWriterLockSlim clientLock;

		public PubSubClient()
		{
			observers = new List<IObserver<ObservableMessage>>();
			observersLock = new ReaderWriterLockSlim();
			clientLock = new ReaderWriterLockSlim();
		}

		public NetworkModel Model
		{
			get
			{
				return model;
			}
		}

		public bool Download()
		{
			return DownloadModel();
		}

		bool DownloadModel()
		{
			NetworkModelDownload download = new NetworkModelDownload();

			if(!download.Download())
			{
				return false;
			}

			NetworkModel tModel = new NetworkModel(download);
			Interlocked.Exchange(ref model, tModel);

			Notify(new ObservableMessage(EObservableMessageType.NetworkModelChanged));
			return true;
		}

		public void Receive(PubSubMessage m)
		{
			switch(m.Topic)
			{
				case ETopic.NetworkModelChanged:
					DownloadModel();
					break;
			}
		}

		public bool Subscribe(IObserver<ObservableMessage> observer)
		{
			if(observer == null)
				return false;

			observersLock.EnterWriteLock();

			try
			{
				if(observers.Contains(observer))
					return false;

				observers.Add(observer);
				return true;
			}
			finally
			{
				observersLock.ExitWriteLock();
			}
		}

		public bool Unsubscribe(IObserver<ObservableMessage> observer)
		{
			if(observer == null)
				return false;

			observersLock.EnterWriteLock();

			try
			{
				return observers.Remove(observer);
			}
			finally
			{
				observersLock.ExitWriteLock();
			}
		}

		void Notify(ObservableMessage message)
		{
			observersLock.EnterReadLock();

			try
			{
				foreach(IObserver<ObservableMessage> observer in observers)
				{
					observer.Notify(message);
				}
			}
			finally
			{
				observersLock.ExitReadLock();
			}
		}

		public bool Connect()
		{
			clientLock.EnterWriteLock();

			try
			{
				DuplexClient<ISubscribing, IPubSubClient> client = new DuplexClient<ISubscribing, IPubSubClient>("callbackEndpoint", this);
				client.Connect();

				if(!client.Call<bool>(sub => { sub.Subscribe(ETopic.NetworkModelChanged); return true; }, out _))
				{
					return false;
				}

				this.client = client;
			}
			finally
			{
				clientLock.ExitWriteLock();
			}

			DownloadModel();
			return true;
		}

		public void Disconnect()
		{
			clientLock.EnterWriteLock();

			try
			{
				if(client == null)
					return;

				client.Disconnect();
				client = null;
			}
			finally
			{
				clientLock.ExitWriteLock();
			}
		}
	}
}
