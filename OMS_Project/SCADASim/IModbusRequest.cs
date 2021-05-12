using Common.SCADA;

namespace SCADASim
{
	public interface IModbusRequest
	{
		ushort TransactionId { get; }
		ushort ProtocolId { get; }
		ushort Length { get; }
		byte UnitId { get; }
		EModbusFunctionCode FunctionCode { get; }

		byte[] PackResponse();
	}
}