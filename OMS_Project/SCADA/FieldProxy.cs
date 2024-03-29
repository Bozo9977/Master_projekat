﻿using Common.CalculationEngine;
using Common.DataModel;
using Common.GDA;
using Common.PubSub;
using Common.SCADA;
using Common.WCF;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace SCADA
{
	public class FieldProxy : Common.IObserver<IReadOnlyDictionary<Tuple<EPointType, ushort>, ushort>>
	{
		static volatile FieldProxy instance;

		ReaderWriterLockSlim modelLock;
		ManualResetEvent stopSignal;
		Thread acquisitionThread;
		SCADAModel model;
		ConcurrentDictionary<ushort, ushort> inputRegisters;
		ConcurrentDictionary<ushort, ushort> holdingRegisters;
		ConcurrentDictionary<ushort, bool> discreteInputs;
		ConcurrentDictionary<ushort, bool> coils;
		ModbusTCPClient modbusClient;
		List<IModbusFunction> acquisitionFunctions;
		Dictionary<int, List<long>> byAddress;

		public static FieldProxy Instance
		{
			get
			{
				return instance;
			}
			set
			{
				instance = value;
			}
		}

		public FieldProxy()
		{
			modelLock = new ReaderWriterLockSlim();
			stopSignal = new ManualResetEvent(false);
			modbusClient = new ModbusTCPClient(new IPEndPoint(IPAddress.Loopback, 502));
			modbusClient.Subscribe(this);
			acquisitionFunctions = new List<IModbusFunction>();
			inputRegisters = new ConcurrentDictionary<ushort, ushort>();
			holdingRegisters = new ConcurrentDictionary<ushort, ushort>();
			discreteInputs = new ConcurrentDictionary<ushort, bool>();
			coils = new ConcurrentDictionary<ushort, bool>();
			byAddress = new Dictionary<int, List<long>>();
		}

		public List<KeyValuePair<long, float>> ReadAnalog(List<long> gids)
		{
			List<KeyValuePair<long, float>> result = new List<KeyValuePair<long, float>>(gids.Count);

			foreach(long gid in gids)
			{
				Analog a = model.GetAnalog(gid);

				if(a == null || a.BaseAddress < 0 || a.BaseAddress > ushort.MaxValue / 2)
					continue;

				ushort address = (ushort)(a.BaseAddress * 2);
				ushort high, low;

				if(!inputRegisters.TryGetValue(address, out high) || !inputRegisters.TryGetValue((ushort)(address + 1), out low))
					continue;

				float value;
				GetValues(high, low, out value, out _);

				result.Add(new KeyValuePair<long, float>(gid, value));
			}

			return result;
		}

		public List<KeyValuePair<long, int>> ReadDiscrete(List<long> gids)
		{
			List<KeyValuePair<long, int>> result = new List<KeyValuePair<long, int>>(gids.Count);

			foreach(long gid in gids)
			{
				Discrete d = model.GetDiscrete(gid);

				if(d == null || d.BaseAddress < 0 || d.BaseAddress > ushort.MaxValue / 2)
					continue;

				ushort address = (ushort)(d.BaseAddress * 2);
				ushort high, low;

				if(!inputRegisters.TryGetValue(address, out high) || !inputRegisters.TryGetValue((ushort)(address + 1), out low))
					continue;

				int value;
				GetValues(high, low, out _, out value);

				result.Add(new KeyValuePair<long, int>(gid, value));
			}

			return result;
		}

		public void CommandAnalog(List<long> gids, List<float> values)
		{
			for(int i = 0; i < Math.Min(gids.Count, values.Count); ++i)
			{
				Analog a = model.GetAnalog(gids[i]);

				if(a == null || a.BaseAddress < 0 || a.BaseAddress > ushort.MaxValue / 2)
					continue;

				ushort address = (ushort)(a.BaseAddress * 2);
				ushort high, low;
				DecomposeAnalogValue(values[i], out high, out low);

				modbusClient.EnqueueCommand(new WriteSingleRegisterFunction(new ModbusWriteCommandParameters(6, (byte)EModbusFunctionCode.WRITE_SINGLE_REGISTER, address, high, 0, 1)));
				modbusClient.EnqueueCommand(new WriteSingleRegisterFunction(new ModbusWriteCommandParameters(6, (byte)EModbusFunctionCode.WRITE_SINGLE_REGISTER, (ushort)(address + 1), low, 0, 1)));
			}
		}

		public void CommandDiscrete(List<long> gids, List<int> values)
		{
			for(int i = 0; i < Math.Min(gids.Count, values.Count); ++i)
			{
				Discrete d = model.GetDiscrete(gids[i]);

				if(d == null || d.BaseAddress < 0 || d.BaseAddress > ushort.MaxValue / 2)
					continue;

				ushort address = (ushort)(d.BaseAddress * 2);
				ushort high, low;
				DecomposeDiscreteValue(values[i], out high, out low);

				modbusClient.EnqueueCommand(new WriteSingleRegisterFunction(new ModbusWriteCommandParameters(6, (byte)EModbusFunctionCode.WRITE_SINGLE_REGISTER, address, high, 0, 1)));
				modbusClient.EnqueueCommand(new WriteSingleRegisterFunction(new ModbusWriteCommandParameters(6, (byte)EModbusFunctionCode.WRITE_SINGLE_REGISTER, (ushort)(address + 1), low, 0, 1)));
			}
		}

		bool IsValidAddress(int baseAddress)
		{
			return baseAddress > 0 && baseAddress <= ushort.MaxValue / 2;
		}

		ushort GetAddress(int baseAddress)
		{
			return (ushort)(baseAddress * 2);
		}

		public void UpdateModel()
		{
			SCADAModel model = SCADAModel.Instance;

			if(model == null)
				return;

			Dictionary<int, List<long>> byAddress = new Dictionary<int, List<long>>();
			List<ushort> inputAddresses = new List<ushort>();

			foreach(Analog a in model.GetAllAnalogs())
			{
				if(a == null || !IsValidAddress(a.BaseAddress))
					continue;

				List<long> gids;
				if(byAddress.TryGetValue(a.BaseAddress, out gids))
				{
					gids.Add(a.GID);
				}
				else
				{
					byAddress[a.BaseAddress] = new List<long>(1) { a.GID };
				}

				if(a.Direction == SignalDirection.Write)
					continue;

				ushort address = GetAddress(a.BaseAddress);
				inputAddresses.Add(address);
			}

			foreach(Discrete d in model.GetAllDiscretes())
			{
				if(d == null || !IsValidAddress(d.BaseAddress))
					continue;

				List<long> gids;
				if(byAddress.TryGetValue(d.BaseAddress, out gids))
				{
					gids.Add(d.GID);
				}
				else
				{
					byAddress[d.BaseAddress] = new List<long>(1) { d.GID };
				}

				if(d.Direction == SignalDirection.Write)
					continue;

				ushort address = GetAddress(d.BaseAddress);
				inputAddresses.Add(address);
			}

			List<IModbusFunction> acquisitionFunctions = new List<IModbusFunction>();

			if(inputAddresses.Count <= 0)
				return;

			inputAddresses.Sort();

			int runStart = inputAddresses[0];
			int runCount = 0;

			for(int i = 0; i < inputAddresses.Count; ++i)
			{
				int address = inputAddresses[i];

				if(runCount < 124 && address == runStart + runCount)
				{
					runCount += 2;
				}
				else if(runCount > 0)
				{
					acquisitionFunctions.Add(new ReadInputRegistersFunction(new ModbusReadCommandParameters(6, (byte)EModbusFunctionCode.READ_INPUT_REGISTERS, (ushort)runStart, (ushort)runCount, 0, 1)));
					runCount = 2;
					runStart = address;
				}
			}

			if(runCount > 0)
			{
				acquisitionFunctions.Add(new ReadInputRegistersFunction(new ModbusReadCommandParameters(6, (byte)EModbusFunctionCode.READ_INPUT_REGISTERS, (ushort)runStart, (ushort)runCount, 0, 1)));
			}

			modelLock.EnterWriteLock();
			{
				this.model = model;
				this.acquisitionFunctions = acquisitionFunctions;
				this.byAddress = byAddress;
			}
			modelLock.ExitWriteLock();

			List<KeyValuePair<long, float>> analogInputs = new List<KeyValuePair<long, float>>();
			List<KeyValuePair<long, float>> analogOutputs = new List<KeyValuePair<long, float>>();
			List<KeyValuePair<long, int>> discreteInputs = new List<KeyValuePair<long, int>>();
			List<KeyValuePair<long, int>> discreteOutputs = new List<KeyValuePair<long, int>>();

			foreach(KeyValuePair<int, List<long>> addressEntry in byAddress)
			{
				ushort address = GetAddress(addressEntry.Key);
				ushort high, low;
				float analogIn;
				int discreteIn;
				float analogOut;
				int discreteOut;

				inputRegisters.TryGetValue(address, out high);
				inputRegisters.TryGetValue((ushort)(address + 1), out low);
				GetValues(high, low, out analogIn, out discreteIn);

				holdingRegisters.TryGetValue(address, out high);
				holdingRegisters.TryGetValue((ushort)(address + 1), out low);
				GetValues(high, low, out analogOut, out discreteOut);

				for(int i = 0; i < addressEntry.Value.Count; ++i)
				{
					long gid = addressEntry.Value[i];

					switch(ModelCodeHelper.GetTypeFromGID(gid))
					{
						case DMSType.Analog:
							Analog a = model.GetAnalog(gid);

							if(a == null)
								continue;

							switch(a.Direction)
							{
								case SignalDirection.Read:
									analogInputs.Add(new KeyValuePair<long, float>(gid, analogIn));
									break;

								case SignalDirection.Write:
									analogOutputs.Add(new KeyValuePair<long, float>(gid, analogOut));
									break;

								case SignalDirection.ReadWrite:
									analogInputs.Add(new KeyValuePair<long, float>(gid, analogIn));
									analogOutputs.Add(new KeyValuePair<long, float>(gid, analogOut));
									break;

								default:
									continue;
							}

							break;

						case DMSType.Discrete:
							Discrete d = model.GetDiscrete(gid);

							if(d == null)
								continue;

							switch(d.Direction)
							{
								case SignalDirection.Read:
									discreteInputs.Add(new KeyValuePair<long, int>(gid, discreteIn));
									break;

								case SignalDirection.Write:
									discreteOutputs.Add(new KeyValuePair<long, int>(gid, discreteOut));
									break;

								case SignalDirection.ReadWrite:
									discreteInputs.Add(new KeyValuePair<long, int>(gid, discreteIn));
									discreteOutputs.Add(new KeyValuePair<long, int>(gid, discreteOut));
									break;

								default:
									continue;
							}

							break;

						default:
							continue;
					}
				}
			}

			PublishMeasurementValues(analogInputs, analogOutputs, discreteInputs, discreteOutputs);
		}

		public bool Start()
		{
			Stop();

			if(!modbusClient.Start())
				return false;

			stopSignal.Reset();
			acquisitionThread = new Thread(Acquisition);
			acquisitionThread.Start();
			return true;
		}

		public void Stop()
		{
			if(acquisitionThread == null)
				return;

			stopSignal.Set();
			acquisitionThread.Join();
			acquisitionThread = null;
			modbusClient.Stop();
		}

		void Acquisition()
		{
			while(!stopSignal.WaitOne(1000))
			{
				List<IModbusFunction> functions;

				modelLock.EnterReadLock();
				{
					functions = this.acquisitionFunctions;
				}
				modelLock.ExitReadLock();

				for(int i = 0; i < functions.Count; ++i)
				{
					modbusClient.EnqueueCommand(functions[i]);
				}
			}
		}

		public void Notify(IReadOnlyDictionary<Tuple<EPointType, ushort>, ushort> message)
		{
			Dictionary<long, float> updatedAnalogInputs = new Dictionary<long, float>();
			Dictionary<long, float> updatedAnalogOutputs = new Dictionary<long, float>();
			Dictionary<long, int> updatedDiscreteInputs = new Dictionary<long, int>();
			Dictionary<long, int> updatedDiscreteOutputs = new Dictionary<long, int>();
			ushort oldAnalogValue;
			bool oldDigitalValue;

			foreach(KeyValuePair<Tuple<EPointType, ushort>, ushort> point in message)
			{
				switch(point.Key.Item1)
				{
					case EPointType.ANALOG_INPUT:
						if(inputRegisters.TryGetValue(point.Key.Item2, out oldAnalogValue) && oldAnalogValue == point.Value)
							continue;

						inputRegisters[point.Key.Item2] = point.Value;

						UpdateGIDsForInputRegister(point.Key.Item2, updatedAnalogInputs, updatedAnalogOutputs, updatedDiscreteInputs, updatedDiscreteOutputs);
						break;

					case EPointType.ANALOG_OUTPUT:
						if(holdingRegisters.TryGetValue(point.Key.Item2, out oldAnalogValue) && oldAnalogValue == point.Value)
							continue;

						holdingRegisters[point.Key.Item2] = point.Value;

						UpdateGIDsForHoldingRegister(point.Key.Item2, updatedAnalogInputs, updatedAnalogOutputs, updatedDiscreteInputs, updatedDiscreteOutputs);
						break;

					case EPointType.DIGITAL_INPUT:
						if(this.discreteInputs.TryGetValue(point.Key.Item2, out oldDigitalValue) && oldDigitalValue == (point.Value != 0))
							continue;

						this.discreteInputs[point.Key.Item2] = point.Value != 0;

						UpdateGIDsForDiscreteInput(point.Key.Item2, updatedAnalogInputs, updatedAnalogOutputs, updatedDiscreteInputs, updatedDiscreteOutputs);
						break;

					case EPointType.DIGITAL_OUTPUT:
						if(coils.TryGetValue(point.Key.Item2, out oldDigitalValue) && oldDigitalValue == (point.Value != 0))
							continue;

						coils[point.Key.Item2] = point.Value != 0;

						UpdateGIDsForCoil(point.Key.Item2, updatedAnalogInputs, updatedAnalogOutputs, updatedDiscreteInputs, updatedDiscreteOutputs);
						break;

					default:
						continue;
				}
			}

			if(updatedAnalogInputs.Count == 0 && updatedAnalogOutputs.Count == 0 && updatedDiscreteInputs.Count == 0 && updatedDiscreteOutputs.Count == 0)
				return;

			PublishMeasurementValues(updatedAnalogInputs, updatedAnalogOutputs, updatedDiscreteInputs, updatedDiscreteOutputs);
		}

		void PublishMeasurementValues(IEnumerable<KeyValuePair<long, float>> analogInputs, IEnumerable<KeyValuePair<long, float>> analogOutputs, IEnumerable<KeyValuePair<long, int>> discreteInputs, IEnumerable<KeyValuePair<long, int>> discreteOutputs)
		{
			List<long> gids = new List<long>();

			foreach(KeyValuePair<long, float> kvp in analogInputs)
			{
				gids.Add(kvp.Key);
			}

			foreach(KeyValuePair<long, int> kvp in discreteInputs)
			{
				gids.Add(kvp.Key);
			}

			Client<ICalculationEngineServiceContract> ceClient = new Client<ICalculationEngineServiceContract>("endpointCE");
			ceClient.Connect();

			ceClient.Call<bool>(ce =>
			{
				ce.UpdateMeasurements(gids);
				return true;
			}, out _);

			ceClient.Disconnect();

			Client<IPublishing> pubClient = new Client<IPublishing>("publishingEndpoint");
			pubClient.Connect();

			pubClient.Call<bool>(pub =>
			{
				pub.Publish(new MeasurementValuesChanged() { AnalogInputs = new List<KeyValuePair<long, float>>(analogInputs), AnalogOutputs = new List<KeyValuePair<long, float>>(analogOutputs), DiscreteInputs = new List<KeyValuePair<long, int>>(discreteInputs), DiscreteOutputs = new List<KeyValuePair<long, int>>(discreteOutputs) });
				return true;
			}, out _);

			pubClient.Disconnect();
		}

		void GetValues(ushort high, ushort low, out float analog, out int discrete)
		{
			byte[] byteValue;
			byte[] highBytes = BitConverter.GetBytes(high);
			byte[] lowBytes = BitConverter.GetBytes(low);

			byteValue = BitConverter.IsLittleEndian ? new byte[4] { lowBytes[0], lowBytes[1], highBytes[0], highBytes[1] } : new byte[4] { highBytes[1], highBytes[0], lowBytes[1], lowBytes[0] };

			analog = BitConverter.ToSingle(byteValue, 0);
			discrete = BitConverter.ToInt32(byteValue, 0);
		}

		void DecomposeAnalogValue(float value, out ushort high, out ushort low)
		{
			DecomposeValue(BitConverter.GetBytes(value), out high, out low);
		}

		void DecomposeDiscreteValue(int value, out ushort high, out ushort low)
		{
			DecomposeValue(BitConverter.GetBytes(value), out high, out low);
		}

		void DecomposeValue(byte[] byteValue, out ushort high, out ushort low)
		{
			if(BitConverter.IsLittleEndian)
			{
				high = BitConverter.ToUInt16(byteValue, 2);
				low = BitConverter.ToUInt16(byteValue, 0);
			}
			else
			{
				high = BitConverter.ToUInt16(new byte[2] { byteValue[1], byteValue[0] }, 0);
				low = BitConverter.ToUInt16(new byte[2] { byteValue[3], byteValue[2] }, 0);
			}
		}

		void UpdateGIDsForInputRegister(ushort address, Dictionary<long, float> updatedAnalogInputs, Dictionary<long, float> updatedAnalogOutputs, Dictionary<long, int> updatedDiscreteInputs, Dictionary<long, int> updatedDiscreteOutputs)
		{
			address -= (ushort)(address % 2);
			ushort high, low;

			if(!inputRegisters.TryGetValue(address, out high) || !inputRegisters.TryGetValue((ushort)(address + 1), out low))
				return;

			float analogValue;
			int discreteValue;
			GetValues(high, low, out analogValue, out discreteValue);

			int baseAddress = address / 2;
			List<long> gids;

			if(!byAddress.TryGetValue(baseAddress, out gids))
				return;

			foreach(long gid in gids)
			{
				DMSType type = ModelCodeHelper.GetTypeFromGID(gid);

				if(type == DMSType.Analog)
				{
					Analog a = model.GetAnalog(gid);

					if(a == null)
						continue;

					switch(a.Direction)
					{
						case SignalDirection.Read:
						case SignalDirection.ReadWrite:
							updatedAnalogInputs[gid] = analogValue;
							break;

						case SignalDirection.Write:
						default:
							continue;
					}
				}
				else if(type == DMSType.Discrete)
				{
					Discrete d = model.GetDiscrete(gid);

					if(d == null)
						continue;

					switch(d.Direction)
					{
						case SignalDirection.Read:
						case SignalDirection.ReadWrite:
							updatedDiscreteInputs[gid] = discreteValue;
							break;

						case SignalDirection.Write:
						default:
							continue;
					}
				}
			}
		}

		void UpdateGIDsForHoldingRegister(ushort address, Dictionary<long, float> updatedAnalogInputs, Dictionary<long, float> updatedAnalogOutputs, Dictionary<long, int> updatedDiscreteInputs, Dictionary<long, int> updatedDiscreteOutputs)
		{
			address -= (ushort)(address % 2);
			ushort high, low;

			if(!holdingRegisters.TryGetValue(address, out high) || !holdingRegisters.TryGetValue((ushort)(address + 1), out low))
				return;

			float analogValue;
			int discreteValue;
			GetValues(high, low, out analogValue, out discreteValue);

			int baseAddress = address / 2;
			List<long> gids;

			if(!byAddress.TryGetValue(baseAddress, out gids))
				return;

			foreach(long gid in gids)
			{
				DMSType type = ModelCodeHelper.GetTypeFromGID(gid);

				if(type == DMSType.Analog)
				{
					Analog a = model.GetAnalog(gid);

					if(a == null)
						continue;

					switch(a.Direction)
					{
						case SignalDirection.Write:
						case SignalDirection.ReadWrite:
							updatedAnalogOutputs[gid] = analogValue;
							break;

						case SignalDirection.Read:
						default:
							continue;
					}
				}
				else if(type == DMSType.Discrete)
				{
					Discrete d = model.GetDiscrete(gid);

					if(d == null)
						continue;

					switch(d.Direction)
					{
						case SignalDirection.Write:
						case SignalDirection.ReadWrite:
							updatedDiscreteOutputs[gid] = discreteValue;
							break;

						case SignalDirection.Read:
						default:
							continue;
					}
				}
			}
		}

		void UpdateGIDsForDiscreteInput(ushort address, Dictionary<long, float> updatedAnalogInputs, Dictionary<long, float> updatedAnalogOutputs, Dictionary<long, int> updatedDiscreteInputs, Dictionary<long, int> updatedDiscreteOutputs)
		{ }

		void UpdateGIDsForCoil(ushort address, Dictionary<long, float> updatedAnalogInputs, Dictionary<long, float> updatedAnalogOutputs, Dictionary<long, int> updatedDiscreteInputs, Dictionary<long, int> updatedDiscreteOutputs)
		{ }
	}
}