using Common;
using Common.PubSub;
using Common.WCF;
using System.Collections.Generic;
using System.Threading;

namespace GUI
{
	public enum EObservableMessageType { NetworkModelChanged, MeasurementValuesChanged }

	public class ObservableMessage
	{
		public EObservableMessageType Type { get; private set; }

		public ObservableMessage(EObservableMessageType type)
		{
			Type = type;
		}
	}

	public class PubSubClient : IPubSubClient, IObservable<ObservableMessage>
	{
		NetworkModel model;
		Measurements measurements;

		List<IObserver<ObservableMessage>> observers;
		ReaderWriterLockSlim observersLock;

		DuplexClient<ISubscribing, IPubSubClient> client;
		ReaderWriterLockSlim clientLock;

		public PubSubClient()
		{
			observers = new List<IObserver<ObservableMessage>>();
			observersLock = new ReaderWriterLockSlim();
			clientLock = new ReaderWriterLockSlim();
			measurements = new Measurements();
		}

		public NetworkModel Model
		{
			get
			{
				return model;
			}
		}

		public Measurements Measurements
		{
			get
			{
				return measurements;
			}
		}

		public bool Connected
		{
			get
			{
				clientLock.EnterReadLock();

				try
				{
					return client != null;
				}
				finally
				{
					clientLock.ExitReadLock();
				}
			}
		}

		public bool Download()
		{
			return HandleNetworkModelChange(null);
		}

		bool HandleNetworkModelChange(NetworkModelChanged msg)
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

		bool HandleMeasurementValuesChange(MeasurementValuesChanged msg)
		{
			measurements.Update(msg);
			Notify(new ObservableMessage(EObservableMessageType.MeasurementValuesChanged));
			return true;
		}

		public void Receive(PubSubMessage m)
		{
			switch(m.Topic)
			{
				case ETopic.NetworkModelChanged:
					HandleNetworkModelChange((NetworkModelChanged)m);
					break;

				case ETopic.MeasurementValuesChanged:
					HandleMeasurementValuesChange((MeasurementValuesChanged)m);
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

		public bool Reconnect()
		{
			clientLock.EnterWriteLock();

			try
			{
				if(this.client != null)
					this.client.Disconnect();

				DuplexClient<ISubscribing, IPubSubClient> client = new DuplexClient<ISubscribing, IPubSubClient>("callbackEndpoint", this);
				client.Connect();

				if(!client.Call<bool>(sub => { sub.Subscribe(ETopic.NetworkModelChanged); sub.Subscribe(ETopic.MeasurementValuesChanged); return true; }, out _))
				{
					return false;
				}

				this.client = client;
			}
			finally
			{
				clientLock.ExitWriteLock();
			}
			
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
