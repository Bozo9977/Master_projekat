using Common.SCADA;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SCADASim
{
	public interface IModbusServerObserver
	{
		void InputRegistersChanged(ushort address, ushort count);
		void HoldingRegistersChanged(ushort address, ushort count);
	}

	public class ModbusTCPServer
	{
		ManualResetEvent stopSignal;
		Thread communicationThread;
		short[] inputRegisters;
		ReaderWriterLockSlim inputRegistersLock;
		short[] holdingRegisters;
		ReaderWriterLockSlim holdingRegistersLock;
		List<IModbusServerObserver> observers;
		ReaderWriterLockSlim observersLock;

		public ModbusTCPServer()
		{
			stopSignal = new ManualResetEvent(false);
			inputRegisters = new short[ushort.MaxValue + 1];
			inputRegistersLock = new ReaderWriterLockSlim();
			holdingRegisters = new short[ushort.MaxValue + 1];
			holdingRegistersLock = new ReaderWriterLockSlim();
			observers = new List<IModbusServerObserver>();
			observersLock = new ReaderWriterLockSlim();
		}

		public bool Start()
		{
			Stop();

			try
			{
				stopSignal.Reset();
				communicationThread = new Thread(CommunicationThread);
				communicationThread.Start();
				return true;
			}
			catch
			{
				communicationThread = null;
				return false;
			}
		}

		public void Stop()
		{
			if(communicationThread != null)
			{
				stopSignal.Set();

				try
				{
					communicationThread.Join();
				}
				catch(Exception e)
				{ }

				communicationThread = null;
			}
		}

		public short GetInputRegister(ushort address)
		{
			inputRegistersLock.EnterReadLock();
			try
			{
				return inputRegisters[address];
			}
			finally
			{
				inputRegistersLock.ExitReadLock();
			}
		}

		public void GetInputRegisters(ushort address, ushort count, short[] output, int offset)
		{
			inputRegistersLock.EnterReadLock();
			try
			{
				Array.Copy(inputRegisters, address, output, offset, count);
			}
			finally
			{
				inputRegistersLock.ExitReadLock();
			}
		}

		public short GetHoldingRegister(ushort address)
		{
			holdingRegistersLock.EnterReadLock();
			try
			{
				return holdingRegisters[address];
			}
			finally
			{
				holdingRegistersLock.ExitReadLock();
			}
		}

		public void GetHoldingRegisters(ushort address, ushort count, short[] output, int offset)
		{
			holdingRegistersLock.EnterReadLock();
			try
			{
				Array.Copy(holdingRegisters, address, output, offset, count);
			}
			finally
			{
				holdingRegistersLock.ExitReadLock();
			}
		}

		public void SetInputRegister(ushort address, short value)
		{
			inputRegistersLock.EnterWriteLock();
			try
			{
				inputRegisters[address] = value;
			}
			finally
			{
				inputRegistersLock.ExitWriteLock();
			}

			Notify(o => o.InputRegistersChanged(address, 1));
		}

		public void SetInputRegisters(ushort address, ushort count, short[] input, int offset)
		{
			inputRegistersLock.EnterWriteLock();
			try
			{
				Array.Copy(input, offset, inputRegisters, address, count);
			}
			finally
			{
				inputRegistersLock.ExitWriteLock();
			}

			Notify(o => o.InputRegistersChanged(address, count));
		}

		public void SetHoldingRegister(ushort address, short value)
		{
			holdingRegistersLock.EnterWriteLock();
			try
			{
				holdingRegisters[address] = value;
			}
			finally
			{
				holdingRegistersLock.ExitWriteLock();
			}

			Notify(o => o.HoldingRegistersChanged(address, 1));
		}

		public void SetHoldingRegisters(ushort address, ushort count, short[] input, int offset)
		{
			holdingRegistersLock.EnterWriteLock();
			try
			{
				Array.Copy(input, offset, holdingRegisters, address, count);
			}
			finally
			{
				holdingRegistersLock.ExitWriteLock();
			}

			Notify(o => o.HoldingRegistersChanged(address, count));
		}

		public void Subscribe(IModbusServerObserver o)
		{
			observersLock.EnterWriteLock();
			try
			{
				if(!observers.Contains(o))
					observers.Add(o);
			}
			finally
			{
				observersLock.ExitWriteLock();
			}
		}

		void Unsubscribe(IModbusServerObserver o)
		{
			observersLock.EnterWriteLock();
			try
			{
				observers.Remove(o);
			}
			finally
			{
				observersLock.ExitWriteLock();
			}
		}

		void Notify(Action<IModbusServerObserver> f)
		{
			observersLock.EnterReadLock();
			try
			{
				foreach(IModbusServerObserver o in observers)
				{
					Task.Run(() => f(o));
				}
			}
			finally
			{
				observersLock.ExitReadLock();
			}
		}

		void CommunicationThread()
		{
			Socket listenSocket;

			try
			{
				listenSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				listenSocket.Bind(new IPEndPoint(IPAddress.Loopback, 502));
				listenSocket.Listen(1);
				listenSocket.Blocking = false;
			}
			catch(Exception e)
			{
				return;
			}

			Socket socket = null;
			byte[] header = new byte[7];
			byte[] data = null;
			int headOffset = int.MaxValue;
			int recvOffset = int.MaxValue;
			int sendOffset = int.MaxValue;

			while(!stopSignal.WaitOne(0))
			{
				if(socket == null)
				{
					try
					{
						socket = Accept(listenSocket, 100000);

						if(socket == null)
							continue;

						socket.Blocking = false;
						data = header;
						headOffset = 0;
						recvOffset = int.MaxValue;
						sendOffset = int.MaxValue;
					}
					catch(Exception e)
					{
						break;
					}
				}

				try
				{
					if(headOffset != int.MaxValue)
					{
						headOffset += Receive(socket, 100000, data, headOffset);

						if(headOffset >= data.Length)
						{
							headOffset = int.MaxValue;
							recvOffset = data.Length;
							int payloadSize = unchecked((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(header, 4)));

							if(payloadSize > 1)
							{
								data = new byte[6 + payloadSize];
								Array.Copy(header, 0, data, 0, recvOffset);
							}
							else
							{
								recvOffset = int.MaxValue;
							}
						}
					}

					if(recvOffset != int.MaxValue)
					{
						recvOffset += Receive(socket, 100000, data, recvOffset);

						if(recvOffset >= data.Length)
						{
							recvOffset = int.MaxValue;
							data = HandleRequest(data);
							sendOffset = 0;
						}
					}

					if(sendOffset != int.MaxValue)
					{
						sendOffset += Send(socket, 100000, data, sendOffset);

						if(sendOffset >= data.Length)
						{
							sendOffset = int.MaxValue;
							headOffset = 0;
							data = header;
						}
					}
				}
				catch(Exception e)
				{
					try
					{
						socket.Shutdown(SocketShutdown.Both);
					}
					catch
					{ }

					socket.Close();
					socket = null;
				}
			}

			try
			{
				listenSocket.Shutdown(SocketShutdown.Both);
			}
			catch
			{ }

			listenSocket.Close();
		}

		byte[] HandleRequest(byte[] data)
		{
			IModbusRequest request;

			switch((EModbusFunctionCode)data[7])
			{
				case EModbusFunctionCode.READ_INPUT_REGISTERS:
					request = new ReadInputRegistersRequest(data, this);
					break;

				case EModbusFunctionCode.WRITE_SINGLE_REGISTER:
					request = new WriteSingleRegisterRequest(data, this);
					break;

				default:
					request = new ModbusRequest(data, this);
					break;
			}

			return request.PackResponse();
		}

		Socket Accept(Socket socket, int microsecondsTimeout)
		{
			if(!socket.Poll(microsecondsTimeout, SelectMode.SelectRead))
				return null;

			return socket.Accept();
		}

		int Send(Socket socket, int microsecondsTimeout, byte[] data, int offset)
		{
			if(!socket.Poll(microsecondsTimeout, SelectMode.SelectWrite))
				return 0;

			return socket.Send(data, offset, data.Length - offset, SocketFlags.None);
		}

		int Receive(Socket socket, int microsecondsTimeout, byte[] data, int offset)
		{
			if(!socket.Poll(microsecondsTimeout, SelectMode.SelectRead))
				return 0;

			return socket.Receive(data, offset, data.Length - offset, SocketFlags.None);
		}
	}
}
