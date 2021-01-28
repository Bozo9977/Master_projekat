namespace Modbus.FunctionParameters
{
    /// <summary>
    /// Class containing parameters for modbus write commands.
    /// </summary>
	public class ModbusWriteCommandParameters : ModbusCommandParameters
	{
		private ushort outputAddress;
		private ushort value;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModbusWriteCommandParameters"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="functionCode">The function code.</param>
        /// <param name="outputAddress">The output address.</param>
        /// <param name="value">The value.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="unitId">The unit identifier.</param>
		public ModbusWriteCommandParameters(ushort length, byte functionCode, ushort outputAddress, ushort value, ushort transactionId, byte unitId)
			: base(length, functionCode, transactionId, unitId)
		{
			OutputAddress = outputAddress;
			Value = value;
		}

        /// <summary>
        /// Gets the output address.
        /// </summary>
		public ushort OutputAddress
		{
			get
			{
				return outputAddress;
			}

			private set
			{
				outputAddress = value;
			}
		}

        /// <summary>
        /// Gets the value.
        /// </summary>
		public ushort Value
		{
			get
			{
				return value;
			}

			private set
			{
				this.value = value;
			}
		}
	}
}