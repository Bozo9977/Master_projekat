namespace SCADA
{
	/// <summary>
	/// Class containing parameters for modbus write commands.
	/// </summary>
	public class ModbusWriteCommandParameters : ModbusCommandParameters
	{
		public ModbusWriteCommandParameters(ushort length, byte functionCode, ushort outputAddress, ushort value, ushort transactionId, byte unitId)
			: base(length, functionCode, transactionId, unitId)
		{
			OutputAddress = outputAddress;
			Value = value;
		}

		public ushort OutputAddress { get; private set; }
		public ushort Value { get; private set; }
	}
}