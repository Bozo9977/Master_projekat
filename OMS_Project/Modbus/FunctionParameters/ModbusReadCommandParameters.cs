namespace Modbus.FunctionParameters
{
    /// <summary>
    /// Class containing parameters for modbus read commands.
    /// </summary>
    public class ModbusReadCommandParameters : ModbusCommandParameters
	{
		private ushort startAddress;
		private ushort quantity;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModbusReadCommandParameters"/> class.
        /// </summary>
        /// <param name="length">The length of the request.</param>
        /// <param name="functionCode">The function code.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="quantity">The quantity.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="unitId">The unit identifier.</param>
		public ModbusReadCommandParameters(ushort length, byte functionCode, ushort startAddress, ushort quantity, ushort transactionId, byte unitId)
				: base(length, functionCode, transactionId, unitId)
		{
			StartAddress = startAddress;
			Quantity = quantity;
		}

        /// <summary>
        /// Gets the start address.
        /// </summary>
		public ushort StartAddress
		{
			get
			{
				return startAddress;
			}

			private set
			{
				startAddress = value;
			}
		}

        /// <summary>
        /// Gets the quantity.
        /// </summary>
		public ushort Quantity
		{
			get
			{
				return quantity;
			}

			private set
			{
				quantity = value;
			}
		}
	}
}