using Common;
using Common.DataModel;
using Common.GDA;
using Common.PubSub;
using Common.SCADA;
using Common.WCF;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GUI
{
	public enum EObservableMessageType { NetworkModelChanged, MeasurementValuesChanged, TopologyChanged, SwitchStatusChanged, LoadFlowChanged }

	public class ObservableMessage
	{
		public EObservableMessageType Type { get; private set; }

		public ObservableMessage(EObservableMessageType type)
		{
			Type = type;
		}
	}

	public class PubSubClient : IPubSubClient, Common.IObservable<ObservableMessage>
	{
		NetworkModel model;
		Topology topology;
		Measurements measurements;

		List<Common.IObserver<ObservableMessage>> observers;
		ReaderWriterLockSlim observersLock;

		DuplexClient<ISubscribing, IPubSubClient> client;
		ReaderWriterLockSlim clientLock;

		public PubSubClient()
		{
			observers = new List<Common.IObserver<ObservableMessage>>();
			observersLock = new ReaderWriterLockSlim();
			clientLock = new ReaderWriterLockSlim();
			topology = new Topology();
			measurements = new Measurements();
		}

		public NetworkModel Model
		{
			get
			{
				return model;
			}
		}

		public Topology Topology
		{
			get
			{
				return topology;
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
			return HandleNetworkModelChange(null) & HandleTopologyChange(null) & DownloadMeasurements();
		}

		bool DownloadMeasurements()
		{
			NetworkModel model = this.Model;
			List<long> analogs = new List<long>(model.GetGIDsByType(DMSType.Analog));
			List<long> discretes = new List<long>(model.GetGIDsByType(DMSType.Discrete));
			List<KeyValuePair<long, float>> analogValues = null;
			List<KeyValuePair<long, int>> discreteValues = null;

			Client<ISCADAServiceContract> client = new Client<ISCADAServiceContract>("endpointSCADA");
			client.Connect();

			if(!client.Call<bool>(scada => { analogValues = scada.ReadAnalog(analogs); discreteValues = scada.ReadDiscrete(discretes); return true; }, out _))
			{
				client.Disconnect();
				return false;
			}

			client.Disconnect();

			measurements.Update(new MeasurementValuesChanged() { AnalogInputs = analogValues, AnalogOutputs = new List<KeyValuePair<long, float>>(0), DiscreteInputs = discreteValues, DiscreteOutputs = new List<KeyValuePair<long, int>>(0) });
			Notify(new ObservableMessage(EObservableMessageType.MeasurementValuesChanged));
			return true;
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
			NetworkModel model = this.Model;

			foreach(KeyValuePair<long, int> dInput in msg.DiscreteInputs)
			{
				IdentifiedObject io = model.Get(dInput.Key);
				Discrete d = io as Discrete;

				if(d == null)
					continue;

				if(d.MeasurementType == MeasurementType.SwitchState)
				{
					Notify(new ObservableMessage(EObservableMessageType.SwitchStatusChanged));
					break;
				}
			}

			Notify(new ObservableMessage(EObservableMessageType.MeasurementValuesChanged));
			return true;
		}

		bool HandleTopologyChange(TopologyChanged m)
		{
			TopologyDownload download = new TopologyDownload();

			if(!download.Download())
			{
				return false;
			}

			topology.Update(download);

			Notify(new ObservableMessage(EObservableMessageType.TopologyChanged));
			return true;
		}

		bool HandleLoadFlowChange(LoadFlowChanged m)
		{
			LoadFlowDownload download = new LoadFlowDownload();

			if(!download.Download())
			{
				return false;
			}

			topology.UpdateLoadFlow(download);
			Notify(new ObservableMessage(EObservableMessageType.LoadFlowChanged));

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

				case ETopic.TopologyChanged:
					HandleTopologyChange((TopologyChanged)m);
					break;

				case ETopic.LoadFlowChanged:
					HandleLoadFlowChange((LoadFlowChanged)m);
					break;
			}
		}

		public bool Subscribe(Common.IObserver<ObservableMessage> observer)
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

		public bool Unsubscribe(Common.IObserver<ObservableMessage> observer)
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
				foreach(Common.IObserver<ObservableMessage> observer in observers)
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

				if(!client.Call<bool>(sub => { sub.Subscribe(ETopic.NetworkModelChanged); sub.Subscribe(ETopic.MeasurementValuesChanged); sub.Subscribe(ETopic.TopologyChanged); return true; }, out _))
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
