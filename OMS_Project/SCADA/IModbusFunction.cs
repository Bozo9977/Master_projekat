using Common.SCADA;
using System;
using System.Collections.Generic;

namespace SCADA
{
	public interface IModbusFunction
	{
		ModbusCommandParameters CommandParameters { get; }
		Dictionary<Tuple<EPointType, ushort>, ushort> ParseResponse(byte[] receivedBytes, out EModbusExceptionCode exceptionCode);
		byte[] PackRequest();
	}
}
