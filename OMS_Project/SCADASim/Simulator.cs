using Common.DataModel;
using Common.GDA;
using Common.PubSub;
using Common.WCF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace SCADASim
{
	class Simulator : IPubSubClient, IModbusServerObserver
	{
		struct CommandParams
		{
			public ushort offset;
			public ushort count;

			public CommandParams(ushort offset, ushort count)
			{
				this.offset = offset;
				this.count = count;
			}
		}

		ModbusTCPServer modbusServer;
		ManualResetEvent stopSignal;
		Dictionary<long, Analog> analogs;
		Dictionary<long, Discrete> discretes;
		Thread simThread;
		HashSet<int> readWriteMeasurementAddresses;
		ConcurrentQueue<CommandParams> commands;
		ReaderWriterLockSlim modelLock;
		DuplexClient<ISubscribing, IPubSubClient> client;

		public Simulator()
		{
			modbusServer = new ModbusTCPServer();
			modbusServer.Subscribe(this);
			stopSignal = new ManualResetEvent(false);
			analogs = new Dictionary<long, Analog>();
			discretes = new Dictionary<long, Discrete>();
			readWriteMeasurementAddresses = new HashSet<int>();
			commands = new ConcurrentQueue<CommandParams>();
			modelLock = new ReaderWriterLockSlim();
			client = new DuplexClient<ISubscribing, IPubSubClient>("callbackEndpoint", this);
		}

		public bool SetDiscreteInput(int address, int value)
		{
			if(address < 0 || address > ushort.MaxValue / 2)
				return false;

			Set32BitInput(BitConverter.GetBytes(value), (ushort)(address * 2));
			return true;
		}

		public bool SetAnalogInput(int address, float value)
		{
			if(address < 0 || address > ushort.MaxValue / 2)
				return false;

			Set32BitInput(BitConverter.GetBytes(value), (ushort)(address * 2));
			return true;
		}

		void Set32BitInput(byte[] byteValue, ushort address)
		{
			if(BitConverter.IsLittleEndian)
			{
				Swap<byte>(ref byteValue[0], ref byteValue[2]);
				Swap<byte>(ref byteValue[1], ref byteValue[3]);
			}

			modbusServer.SetInputRegisters(address, 2, new short[2] { BitConverter.ToInt16(byteValue, 0), BitConverter.ToInt16(byteValue, 2) }, 0);
		}

		static void Swap<T>(ref T x, ref T y)
		{
			T temp = x;
			x = y;
			y = temp;
		}

		public bool Start()
		{
			Stop();
			client.Connect();
			client.Call<bool>(sub => { sub.Subscribe(ETopic.NetworkModelChanged); return true; }, out _);
			DownloadModel();
			StartSimulation();
			return modbusServer.Start();
		}

		public void Stop()
		{
			client.Disconnect();
			modbusServer.Stop();
			StopSimulation();
		}

		public void Receive(PubSubMessage m)
		{
			if(m.Topic != ETopic.NetworkModelChanged)
				return;

			DownloadModel();
		}

		public bool DownloadModel()
		{
			SCADASimModelDownload download = new SCADASimModelDownload();

			if(!download.Download())
				return false;
			
			HashSet<int> readWriteMeasurementAddresses = new HashSet<int>();

			foreach(Analog a in download.Analogs.Values)
			{
				if(a.Direction == SignalDirection.ReadWrite)
					readWriteMeasurementAddresses.Add(a.BaseAddress);
			}

			foreach(Discrete d in download.Discretes.Values)
			{
				if(d.Direction == SignalDirection.ReadWrite)
					readWriteMeasurementAddresses.Add(d.BaseAddress);
			}

			modelLock.EnterWriteLock();
			{
				analogs = download.Analogs;
				discretes = download.Discretes;
				this.readWriteMeasurementAddresses = readWriteMeasurementAddresses;
			}
			modelLock.ExitWriteLock();

			foreach(Analog a in download.Analogs.Values)
			{
				if(a.Direction == SignalDirection.ReadWrite)
					SetAnalogInput(a.BaseAddress, a.NormalValue);
			}

			foreach(Discrete d in download.Discretes.Values)
			{
				if(d.Direction == SignalDirection.ReadWrite)
					SetDiscreteInput(d.BaseAddress, d.NormalValue);
			}

			return true;
		}

		void Simulation()
		{
			Random r = new Random();

			while(!stopSignal.WaitOne(1000))
			{
				Dictionary<long, Analog> analogs;
				Dictionary<long, Discrete> discretes;
				HashSet<int> readWriteMeasurementAddresses;

				modelLock.EnterReadLock();
				{
					analogs = this.analogs;
					discretes = this.discretes;
					readWriteMeasurementAddresses = this.readWriteMeasurementAddresses;
				}
				modelLock.ExitReadLock();

				CommandParams command;
				while(commands.TryDequeue(out command))
				{
					for(ushort address = command.offset; address < command.offset + command.count; ++address)
					{
						if(!readWriteMeasurementAddresses.Contains(address / 2))
							continue;

						modbusServer.SetInputRegister(address, modbusServer.GetHoldingRegister(address));
					}
				}

				foreach(KeyValuePair<long, Analog> a in analogs)
				{
					if(a.Value.Direction != SignalDirection.Read)
						continue;

					float newValue = a.Value.NormalValue + ((float)r.NextDouble() * 10 - 5);
					SetAnalogInput(a.Value.BaseAddress, newValue);
				}

				foreach(KeyValuePair<long, Discrete> d in discretes)
				{
					if(d.Value.Direction != SignalDirection.Read)
						continue;

					int newValue = d.Value.NormalValue + (r.Next(11) - 5);
					SetDiscreteInput(d.Value.BaseAddress, newValue);
				}
			}
		}

		void StartSimulation()
		{
			stopSignal.Reset();
			simThread = new Thread(Simulation);
			simThread.Start();
		}

		void StopSimulation()
		{
			if(simThread == null)
				return;

			stopSignal.Set();
			simThread.Join();
			simThread = null;
		}

		public void InputRegistersChanged(ushort address, ushort count)
		{ }

		public void HoldingRegistersChanged(ushort address, ushort count)
		{
			commands.Enqueue(new CommandParams(address, count));
		}
	}
}
