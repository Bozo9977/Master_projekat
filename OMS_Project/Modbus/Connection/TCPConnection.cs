using SCADA_Common;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Modbus.Connection
{
    /// <summary>
    /// Class containing logic for establisshing tcp connections.
    /// </summary>
    internal class TCPConnection : IConnection
	{
		private IStateUpdater stateUpdater;
		private IPEndPoint remoteEP;
		private Socket socket;
		private IConfiguration configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TCPConnection"/> class.
        /// </summary>
        /// <param name="stateUpdater">The state updater.</param>
        /// <param name="configuration">The configuration.</param>
		public TCPConnection(IStateUpdater stateUpdater, IConfiguration configuration)
		{
			this.stateUpdater = stateUpdater;
			this.configuration = configuration;
			this.remoteEP = CreateRemoteEndpoint();
		}

        /// <inheritdoc />
        public void Connect()
		{
			this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
			this.socket.Blocking = false;
			try
			{
				socket.Connect(remoteEP);
			}
			catch (SocketException se)
			{
				if (se.ErrorCode != 10035)
				{
					throw se;
				}
			}
		}

        /// <inheritdoc />
        public void Disconnect()
		{
			if (socket.Connected)
			{
				socket.Shutdown(SocketShutdown.Both);
			}

			socket.Close();
			socket = null;
		}

        /// <inheritdoc />
        public byte[] RecvBytes(int numberOfBytes)
		{
			int numberOfReceivedBytes = 0;
			byte[] retval = new byte[numberOfBytes];
			int numOfReceived;
			while (numberOfReceivedBytes < numberOfBytes)
			{
				numOfReceived = 0;
				if (socket.Poll(1623, SelectMode.SelectRead))
				{
					numOfReceived = socket.Receive(retval, numberOfReceivedBytes, (int)numberOfBytes - numberOfReceivedBytes, SocketFlags.None);
					if (numOfReceived > 0)
					{
						numberOfReceivedBytes += numOfReceived;
					}
				}
			}
			return retval;
		}

        /// <inheritdoc />
        public void SendBytes(byte[] bytesToSend)
		{
			int currentlySent = 0;

			while (currentlySent < bytesToSend.Count())
			{
				if (socket.Poll(1623, SelectMode.SelectWrite))
				{
					currentlySent += socket.Send(bytesToSend, currentlySent, bytesToSend.Length - currentlySent, SocketFlags.None);
				}
			}
		}

        /// <summary>
        /// Creates a remote endpoint.
        /// </summary>
        /// <returns>The created endpoint</returns>
		private IPEndPoint CreateRemoteEndpoint()
		{
			IPHostEntry ipHostInfo = Dns.GetHostEntry("localhost");
			IPAddress ipAddress = null;
			foreach (IPAddress ip in ipHostInfo.AddressList)
				if ("127.0.0.1".Equals(ip.ToString()))
					ipAddress = ip;
			return new IPEndPoint(ipAddress, configuration.TcpPort);
		}

        /// <inheritdoc />
		public bool CheckState()
		{
			return this.socket.Poll(30000, SelectMode.SelectWrite);
		}
	}
}