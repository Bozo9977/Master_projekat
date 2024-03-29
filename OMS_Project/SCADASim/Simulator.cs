﻿using Common;
using Common.CalculationEngine;
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
		List<Tuple<Recloser, long, long, int>> reclosers;
		List<DailyLoadProfile> loadProfiles;
		Dictionary<long, EnergyConsumer> energyConsumers;

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
			reclosers = new List<Tuple<Recloser, long, long, int>>();
			loadProfiles = DailyLoadProfile.LoadFromXML("Daily_load_profiles.xml");
			energyConsumers = new Dictionary<long, EnergyConsumer>();
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

			if(!client.Call<bool>(sub => { sub.Subscribe(ETopic.NetworkModelChanged); sub.Subscribe(ETopic.TopologyChanged); return true; }, out _))
			{
				Logger.Instance.Log(ELogLevel.ERROR, "Cannot connect to PubSub.");
			}

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
			switch(m.Topic)
			{
				case ETopic.NetworkModelChanged:
					DownloadModel();
					break;

				case ETopic.TopologyChanged:
					TopologyUpdated();
					break;
			}
		}

		void TopologyUpdated()
		{
			Client<ICalculationEngineServiceContract> client = new Client<ICalculationEngineServiceContract>("endpointCE");
			client.Connect();

			List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> result = null;
			bool success;

			if(!client.Call<bool>(ce => { result = ce.GetLineEnergization(); return true; }, out success) || !success)
			{
				client.Disconnect();
				return;
			}

			client.Disconnect();

			modelLock.EnterReadLock();

			Dictionary<long, EEnergization> cnStates = new Dictionary<long, EEnergization>(reclosers.Count * 2);

			foreach(Tuple<Recloser, long, long, int> r in reclosers)
			{
				cnStates[r.Item2] = EEnergization.NotEnergized;
				cnStates[r.Item3] = EEnergization.NotEnergized;
			}

			List<long> reclosersWithoutSCADA = new List<long>();

			try
			{
				foreach(Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>> source in result)
				{
					foreach(Tuple<long, long> energizedLine in source.Item2)
					{
						if(cnStates.ContainsKey(energizedLine.Item1))
							cnStates[energizedLine.Item1] = EEnergization.Energized;

						if(cnStates.ContainsKey(energizedLine.Item2))
							cnStates[energizedLine.Item2] = EEnergization.Energized;
					}

					foreach(Tuple<long, long> unknownLine in source.Item3)
					{
						EEnergization state;

						if(cnStates.TryGetValue(unknownLine.Item1, out state) && state == EEnergization.Unknown)
							cnStates[unknownLine.Item1] = EEnergization.Unknown;

						if(cnStates.TryGetValue(unknownLine.Item2, out state) && state == EEnergization.Unknown)
							cnStates[unknownLine.Item2] = EEnergization.Unknown;
					}
				}

				foreach(Tuple<Recloser, long, long, int> r in reclosers)
				{
					EEnergization[] states = new EEnergization[] { cnStates[r.Item2], cnStates[r.Item3] };

					if(states[0] == EEnergization.Unknown || states[1] == EEnergization.Unknown)
						continue;

					if(states[0] != states[1])
					{
						if(r.Item4 < 0)
						{
							reclosersWithoutSCADA.Add(r.Item1.GID);
						}
						else
						{
							SetDiscreteInput(r.Item4, 0);
						}
					}
				}
			}
			finally
			{
				modelLock.ExitReadLock();
			}

			if(reclosersWithoutSCADA.Count <= 0)
				return;

			client = new Client<ICalculationEngineServiceContract>("endpointCE");
			client.Connect();
			client.Call<bool>(ce => 
			{
				foreach(long gid in reclosersWithoutSCADA)
				{
					ce.MarkSwitchState(gid, false);
				}

				return true;
			}, out success);
			client.Disconnect();
		}

		public bool DownloadModel()
		{
			SCADASimModelDownload download = new SCADASimModelDownload();

			if(!download.Download())
				return false;
			
			List<Pair<int, float>> setAnalogValues = new List<Pair<int, float>>();
			List<Pair<int, int>> setDiscreteValues = new List<Pair<int, int>>();
			HashSet<int> readWriteMeasurementAddresses = new HashSet<int>();

			modelLock.EnterUpgradeableReadLock();

			try
			{
				foreach(Analog a in download.Analogs.Values)
				{
					if(a.Direction == SignalDirection.ReadWrite)
					{
						readWriteMeasurementAddresses.Add(a.BaseAddress);

						Analog oldAnalog;

						if(!analogs.TryGetValue(a.GID, out oldAnalog) || !AreAnalogsEqual(oldAnalog, a))
							setAnalogValues.Add(new Pair<int, float>(a.BaseAddress, a.NormalValue));
					}
				}

				foreach(Discrete d in download.Discretes.Values)
				{
					if(d.Direction == SignalDirection.ReadWrite)
					{
						readWriteMeasurementAddresses.Add(d.BaseAddress);

						Discrete oldDiscrete;

						if(!discretes.TryGetValue(d.GID, out oldDiscrete) || !AreDiscretesEqual(oldDiscrete, d))
							setDiscreteValues.Add(new Pair<int, int>(d.BaseAddress, d.NormalValue));
					}
				}

				List<Tuple<Recloser, long, long, int>> reclosers = new List<Tuple<Recloser, long, long, int>>(download.Reclosers.Count);

				foreach(Recloser r in download.Reclosers.Values)
				{
					int address = -1;
					int i;

					for(i = 0; i < r.Measurements.Count; ++i)
					{
						Discrete d;
					
						if(!download.Discretes.TryGetValue(r.Measurements[i], out d) || d.MeasurementType != MeasurementType.SwitchState || d.Direction == SignalDirection.Write)
							continue;

						address = d.BaseAddress;
						break;
					}

					Terminal t1, t2;

					if(r.Terminals.Count != 2 || !download.Terminals.TryGetValue(r.Terminals[0], out t1) || !download.Terminals.TryGetValue(r.Terminals[1], out t2))
						continue;

					reclosers.Add(new Tuple<Recloser, long, long, int>(r, t1.ConnectivityNode, t2.ConnectivityNode, address));
				}

				modelLock.EnterWriteLock();
				{
					analogs = download.Analogs;
					discretes = download.Discretes;
					energyConsumers = download.EnergyConsumers;
					this.readWriteMeasurementAddresses = readWriteMeasurementAddresses;
					this.reclosers = reclosers;
				}
				modelLock.ExitWriteLock();
			}
			catch(Exception e)
			{
				return false;
			}
			finally
			{
				modelLock.ExitUpgradeableReadLock();
			}

			foreach(Pair<int, float> value in setAnalogValues)
			{
				SetAnalogInput(value.First, value.Second);
			}

			foreach(Pair<int, int> value in setDiscreteValues)
			{
				SetDiscreteInput(value.First, value.Second);
			}

			return true;
		}

		bool AreAnalogsEqual(Analog a1, Analog a2)
		{
			return a1.GID == a2.GID && a1.BaseAddress == a2.BaseAddress && a1.Direction == a2.Direction && a1.MeasurementType == a2.MeasurementType && a1.NormalValue == a2.NormalValue && a1.PowerSystemResource == a2.PowerSystemResource;
		}

		bool AreDiscretesEqual(Discrete d1, Discrete d2)
		{
			return d1.GID == d2.GID && d1.BaseAddress == d2.BaseAddress && d1.Direction == d2.Direction && d1.MeasurementType == d2.MeasurementType && d1.NormalValue == d2.NormalValue && d1.PowerSystemResource == d2.PowerSystemResource;
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

				DateTime now = DateTime.Now;

				foreach(Analog a in analogs.Values)
				{
					if(a.Direction != SignalDirection.Read)
						continue;

					float newValue = float.NaN;
					
					if(ModelCodeHelper.GetTypeFromGID(a.PowerSystemResource) == DMSType.EnergyConsumer)
					{
						newValue = GetConsumerPowerValue(now, a);
					}

					if(float.IsNaN(newValue))
						newValue = a.NormalValue;

					SetAnalogInput(a.BaseAddress, newValue);
				}

				foreach(Discrete d in discretes.Values)
				{
					if(d.Direction != SignalDirection.Read)
						continue;

					int newValue = d.NormalValue + (r.Next(11) - 5);
					SetDiscreteInput(d.BaseAddress, newValue);
				}
			}
		}

		float GetConsumerPowerValue(DateTime t, Analog a)
		{
			EnergyConsumer ec;

			if((a.MeasurementType != MeasurementType.ActivePower && a.MeasurementType != MeasurementType.ReactivePower) || !energyConsumers.TryGetValue(a.PowerSystemResource, out ec))
				return float.NaN;

			DailyLoadProfile loadProfile = loadProfiles.Find(x => x.ConsumerClass == ec.ConsumerClass);

			if(loadProfile == null)
				return float.NaN;

			return loadProfile.Get(t.Hour, t.Minute) * (a.MeasurementType == MeasurementType.ActivePower ? ec.PFixed : ec.QFixed);
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
