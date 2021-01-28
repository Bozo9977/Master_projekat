namespace Modbus.FunctionParameters
{
	public abstract class ModbusCommandParameters
	{
		private ushort transactionId;
		private ushort protocolId;
		private ushort length;
		private byte unitId;
		private byte functionCode;

		/// <summary>
		/// Initializes a new instance of the <see cref="ModbusCommandParameters"/> class.
		/// </summary>
		/// <param name="length">The request length.</param>
		/// <param name="functionCode">The function code.</param>
		/// <param name="transactionId">The transaction identifier.</param>
		/// <param name="unitId">The unit identifier.</param>
		public ModbusCommandParameters(ushort length, byte functionCode, ushort transactionId, byte unitId)
		{
			TransactionId = transactionId;
			UnitId = unitId;

			ProtocolId = 0;
			Length = length;
			FunctionCode = functionCode;
		}

		/// <summary>
		/// Gets the transaction identifier.
		/// </summary>
		public ushort TransactionId
		{
			get
			{
				return transactionId;
			}

			private set
			{
				transactionId = value;
			}
		}

		/// <summary>
		/// Gets the protocol identifier.
		/// </summary>
		public ushort ProtocolId
		{
			get
			{
				return protocolId;
			}

			private set
			{
				protocolId = value;
			}
		}

		/// <summary>
		/// Gets the length of the request.
		/// </summary>
		public ushort Length
		{
			get
			{
				return length;
			}

			private set
			{
				length = value;
			}
		}

		/// <summary>
		/// Gets the unit identifier.
		/// </summary>
		public byte UnitId
		{
			get
			{
				return unitId;
			}

			private set
			{
				unitId = value;
			}
		}

		/// <summary>
		/// Gets the function code.
		/// </summary>
		public byte FunctionCode
		{
			get
			{
				return functionCode;
			}

			private set
			{
				functionCode = value;
			}
		}
	}
}
