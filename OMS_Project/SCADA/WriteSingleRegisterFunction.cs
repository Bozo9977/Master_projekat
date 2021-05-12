using Common.SCADA;
using System;
using System.Collections.Generic;

namespace SCADA
{
	public class WriteSingleRegisterFunction : ModbusFunction
	{
		ModbusWriteCommandParameters param;

		public WriteSingleRegisterFunction(ModbusWriteCommandParameters commandParameters) : base()
		{
			param = commandParameters;
		}

		public override ModbusCommandParameters CommandParameters { get { return param; } }
		public ModbusWriteCommandParameters WriteCommandParameters { get { return param; } }

		public override byte[] PackRequest()
		{
			byte[] packet = new byte[12];

			PackUInt16(param.TransactionId, packet, 0);
			PackUInt16(param.ProtocolId, packet, 2);
			PackUInt16(param.Length, packet, 4);
			packet[6] = param.UnitId;
			packet[7] = param.FunctionCode;
			PackUInt16(param.OutputAddress, packet, 8);
			PackUInt16(param.Value, packet, 10);

			return packet;
		}

		public override Dictionary<Tuple<EPointType, ushort>, ushort> ParseResponse(byte[] response, out EModbusExceptionCode exceptionCode)
		{
			exceptionCode = EModbusExceptionCode.NO_EXCEPTION;
			Dictionary<Tuple<EPointType, ushort>, ushort> parsed = new Dictionary<Tuple<EPointType, ushort>, ushort>(1);

			if(response[7] != param.FunctionCode)
			{
				exceptionCode = GetExceptionCode(response[8]);
				return parsed;
			}

			ushort address = UnpackUInt16(response, 8);
			ushort value = UnpackUInt16(response, 10);
			parsed.Add(new Tuple<EPointType, ushort>(EPointType.ANALOG_OUTPUT, address), value);

			return parsed;
		}
	}
}
