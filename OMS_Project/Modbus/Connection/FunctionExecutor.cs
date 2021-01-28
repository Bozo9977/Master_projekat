using SCADA_Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Modbus.Connection
{
	public class FunctionExecutor : IDisposable, IFunctionExecutor
	{
		private IConnection connection;
		private IStateUpdater stateUpdater;
		private IModbusFunction currentCommand;
		private bool threadCancellationSignal = true;
		private AutoResetEvent processConnection;
		private Thread connectionProcessorThread;
		private ConnectionState connectionState = ConnectionState.DISCONNECTED;
		private uint numberOfConnectionRetries = 0;
		private ConcurrentQueue<IModbusFunction> commandQueue = new ConcurrentQueue<IModbusFunction>();
		private IConfiguration configuration;
		private string RECEIVED_MESSAGE = "Point of type {0} on address {1:d5} received value: {2}";

		/// <inheritdoc />
		public event UpdatePointDelegate UpdatePointEvent;

		/// <summary>
		/// Initializes a new instance of the <see cref="FunctionExecutor"/> class.
		/// </summary>
		/// <param name="stateUpdater">The state updater.</param>
		/// <param name="configuration">The configuration.</param>
		public FunctionExecutor(IStateUpdater stateUpdater, IConfiguration configuration)
		{
			this.stateUpdater = stateUpdater;
			this.configuration = configuration;
			connection = new TCPConnection(stateUpdater, configuration);
			this.processConnection = new AutoResetEvent(false);
			connectionProcessorThread = new Thread(new ThreadStart(ConnectionProcessorThread));
			connectionProcessorThread.Name = "Communication thread";
			connectionProcessorThread.Start();
		}

		/// <inheritdoc />
		public void EnqueueCommand(IModbusFunction commandToExecute)
		{
			if (this.connectionState == ConnectionState.CONNECTED)
			{
				this.commandQueue.Enqueue(commandToExecute);
				this.processConnection.Set();
			}
		}

		/// <summary>
		/// Invokes the update point event after the response is parsed.
		/// </summary>
		/// <param name="receivedBytes">The received response.</param>
		public void HandleReceivedBytes(byte[] receivedBytes)
		{
			Dictionary<Tuple<PointType, ushort>, ushort> pointsToupdate = this.currentCommand?.ParseResponse(receivedBytes);
			if (UpdatePointEvent != null)
			{
				foreach (var point in pointsToupdate)
				{
					UpdatePointEvent.Invoke(point.Key.Item1, point.Key.Item2, point.Value);
					stateUpdater.LogMessage(string.Format(RECEIVED_MESSAGE, point.Key.Item1, point.Key.Item2, point.Value));
				}
			}
		}

		/// <summary>
		/// Logic for handling the connection.
		/// </summary>
		private void ConnectionProcessorThread()
		{
			while (this.threadCancellationSignal)
			{
				try
				{
					if (this.connectionState == ConnectionState.DISCONNECTED)
					{
						numberOfConnectionRetries = 0;
						this.connection.Connect();
						while (numberOfConnectionRetries < 10)
						{
							if (this.connection.CheckState())
							{
								this.connectionState = ConnectionState.CONNECTED;
								this.stateUpdater.UpdateConnectionState(this.connectionState);
								numberOfConnectionRetries = 0;
								break;
							}
							else
							{
								numberOfConnectionRetries++;
								if (numberOfConnectionRetries == 10)
								{
									this.connection.Disconnect();
									this.connectionState = ConnectionState.DISCONNECTED;
									this.stateUpdater.UpdateConnectionState(this.connectionState);
								}
							}
						}
					}
					else
					{
						processConnection.WaitOne();
						while (commandQueue.TryDequeue(out currentCommand))
						{
							this.connection.SendBytes(this.currentCommand.PackRequest());
							byte[] message;
							byte[] header = this.connection.RecvBytes(7);
							int payLoadSize = 0;
							unchecked
							{
								payLoadSize = IPAddress.NetworkToHostOrder((short)BitConverter.ToUInt16(header, 4));
							}
							byte[] payload = this.connection.RecvBytes(payLoadSize - 1);
							message = new byte[header.Length + payload.Length];
							Buffer.BlockCopy(header, 0, message, 0, 7);
							Buffer.BlockCopy(payload, 0, message, 7, payload.Length);
							this.HandleReceivedBytes(message);
							this.currentCommand = null;
						}
					}
				}
				catch (SocketException se)
				{
					if (se.ErrorCode != 10054)
					{
						throw se;
					}
					currentCommand = null;
					this.connectionState = ConnectionState.DISCONNECTED;
					this.stateUpdater.UpdateConnectionState(ConnectionState.DISCONNECTED);
					string message = $"{se.TargetSite.ReflectedType.Name}.{se.TargetSite.Name}: {se.Message}";
					stateUpdater.LogMessage(message);
				}
				catch (Exception ex)
				{
					string message = $"{ex.TargetSite.ReflectedType.Name}.{ex.TargetSite.Name}: {ex.Message}";
					stateUpdater.LogMessage(message);
					currentCommand = null;
				}
			}
		}

		/// <inheritdoc />
		public void Dispose()
		{
			connectionProcessorThread.Abort();
		}
	}
}
