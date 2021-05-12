using Common.SCADA;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SCADA
{
	class ModbusTCPClient : Common.IObservable<IReadOnlyDictionary<Tuple<EPointType, ushort>, ushort>>
	{
		IPEndPoint serverEndpoint;
		Socket socket;
		ConcurrentQueue<IModbusFunction> commandQueue;
		List<Common.IObserver<IReadOnlyDictionary<Tuple<EPointType, ushort>, ushort>>> observers;
		ReaderWriterLockSlim observersLock;
		ManualResetEvent stopSignal;
		Thread communicationThread;
		ReaderWriterLockSlim socketLock;

		public bool Connected 
		{ 
			get
			{
				bool connected;

				socketLock.EnterReadLock();
				{
					connected = socket != null;
				}
				socketLock.ExitReadLock();

				return connected;
			}
		}

		public ModbusTCPClient(IPEndPoint serverEndpoint)
		{
			this.serverEndpoint = serverEndpoint;
			commandQueue = new ConcurrentQueue<IModbusFunction>();
			observers = new List<Common.IObserver<IReadOnlyDictionary<Tuple<EPointType, ushort>, ushort>>>();
			observersLock = new ReaderWriterLockSlim();
			stopSignal = new ManualResetEvent(false);
			socketLock = new ReaderWriterLockSlim();
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

		bool Reconnect()
		{
			socketLock.EnterWriteLock();
			try
			{
				if(this.socket != null)
				{
					try
					{
						this.socket.Shutdown(SocketShutdown.Both);
					}
					catch(Exception e)
					{ }

					this.socket.Close();
					this.socket = null;
				}

				Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
				socket.Connect(serverEndpoint);
				socket.Blocking = false;

				this.socket = socket;

				return true;
			}
			catch(Exception e)
			{
				return false;
			}
			finally
			{
				socketLock.ExitWriteLock();
			}
		}

		void Disconnect()
		{
			socketLock.EnterWriteLock();
			try
			{
				try
				{
					socket.Shutdown(SocketShutdown.Both);
				}
				catch(Exception e)
				{ }

				socket.Close();
				socket = null;
			}
			finally
			{
				socketLock.ExitWriteLock();
			}
		}

		public bool EnqueueCommand(IModbusFunction command)
		{
			commandQueue.Enqueue(command);
			return true;
		}

		public bool Subscribe(Common.IObserver<IReadOnlyDictionary<Tuple<EPointType, ushort>, ushort>> observer)
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

		public bool Unsubscribe(Common.IObserver<IReadOnlyDictionary<Tuple<EPointType, ushort>, ushort>> observer)
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

		void Notify(IReadOnlyDictionary<Tuple<EPointType, ushort>, ushort> message)
		{
			observersLock.EnterReadLock();

			try
			{
				foreach(Common.IObserver<IReadOnlyDictionary<Tuple<EPointType, ushort>, ushort>> observer in observers)
				{
					observer.Notify(message);
				}
			}
			finally
			{
				observersLock.ExitReadLock();
			}
		}

		void HandleResponse(IModbusFunction command, byte[] receivedBytes)
		{
			Dictionary<Tuple<EPointType, ushort>, ushort> pointsToUpdate = command.ParseResponse(receivedBytes, out _);
			Notify(pointsToUpdate);
		}

		void CommunicationThread()
		{
			IModbusFunction command = null;
			byte[] data = null;
			byte[] header = new byte[7];
			int sendOffset = int.MaxValue;
			int headOffset = int.MaxValue;
			int recvOffset = int.MaxValue;

			while(!stopSignal.WaitOne(100))
			{
				if(command == null && commandQueue.TryDequeue(out command))
				{
					data = command.PackRequest();
					sendOffset = 0;
				}

				Socket socket;
				socketLock.EnterReadLock();
				{
					socket = this.socket;
				}
				socketLock.ExitReadLock();

				if(socket == null)
				{
					Reconnect();
					Thread.Sleep(100);
					continue;
				}

				try
				{
					if(sendOffset != int.MaxValue)
					{
						sendOffset += Send(socket, 500000, data, sendOffset);

						if(sendOffset >= data.Length)
						{
							sendOffset = int.MaxValue;
							headOffset = 0;
							data = header;
						}
					}

					if(headOffset != int.MaxValue)
					{
						headOffset += Receive(socket, 500000, data, headOffset);

						if(headOffset >= data.Length)
						{
							headOffset = int.MaxValue;
							recvOffset = 7;
							int payloadSize = unchecked((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(header, 4)));

							if(payloadSize > 1)
							{
								data = new byte[6 + payloadSize];
								Array.Copy(header, 0, data, 0, 7);
							}
							else
							{
								recvOffset = int.MaxValue;
								command = null;
							}
						}
					}

					if(recvOffset != int.MaxValue)
					{
						recvOffset += Receive(socket, 500000, data, recvOffset);

						if(recvOffset >= data.Length)
						{
							recvOffset = int.MaxValue;
							HandleResponse(command, data);
							command = null;
						}
					}
				}
				catch(SocketException e)
				{
					Reconnect();
					Thread.Sleep(100);
				}
			}
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
