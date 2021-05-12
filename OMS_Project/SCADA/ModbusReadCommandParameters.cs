namespace SCADA
{
	public class ModbusReadCommandParameters : ModbusCommandParameters
	{
		public ModbusReadCommandParameters(ushort length, byte functionCode, ushort startAddress, ushort quantity, ushort transactionId, byte unitId)
				: base(length, functionCode, transactionId, unitId)
		{
			StartAddress = startAddress;
			Quantity = quantity;
		}

		public ushort StartAddress { get; private set; }
		public ushort Quantity { get; private set; }
	}
}