namespace SCADA
{
	public abstract class ModbusCommandParameters
	{
		public ModbusCommandParameters(ushort length, byte functionCode, ushort transactionId, byte unitId)
		{
			TransactionId = transactionId;
			UnitId = unitId;
			ProtocolId = 0;
			Length = length;
			FunctionCode = functionCode;
		}

		public ushort TransactionId { get; private set; }
		public ushort ProtocolId { get; private set; }
		public ushort Length { get; private set; }
		public byte UnitId { get; private set; }
		public byte FunctionCode { get; private set; }
	}
}