using Common.SCADA;
using System;
using System.Collections.Generic;

namespace SCADA
{
	public class ReadInputRegistersFunction : ModbusFunction
	{
		ModbusReadCommandParameters param;

		public ReadInputRegistersFunction(ModbusReadCommandParameters commandParameters) : base()
		{
			param = commandParameters;
		}

		public override ModbusCommandParameters CommandParameters { get { return param; } }
		public ModbusReadCommandParameters ReadCommandParameters { get { return param; } }

		public override byte[] PackRequest()
		{
			byte[] packet = new byte[12];

			PackUInt16(param.TransactionId, packet, 0);
			PackUInt16(param.ProtocolId, packet, 2);
			PackUInt16(param.Length, packet, 4);
			packet[6] = param.UnitId;
			packet[7] = param.FunctionCode;
			PackUInt16(param.StartAddress, packet, 8);
			PackUInt16(param.Quantity, packet, 10);

			return packet;
		}

		public override Dictionary<Tuple<EPointType, ushort>, ushort> ParseResponse(byte[] response, out EModbusExceptionCode exceptionCode)
		{
			exceptionCode = EModbusExceptionCode.NO_EXCEPTION;
			Dictionary<Tuple<EPointType, ushort>, ushort> parsed = new Dictionary<Tuple<EPointType, ushort>, ushort>();

			if(response[7] != param.FunctionCode)
			{
				exceptionCode = GetExceptionCode(response[8]);
				return parsed;
			}

			ushort address = param.StartAddress;

			for(int i = 0; i < response[8]; i += 2)
			{
				ushort value = UnpackUInt16(response, i + 9);
				parsed.Add(new Tuple<EPointType, ushort>(EPointType.ANALOG_INPUT, address), value);
				address++;
			}

			return parsed;
		}
	}
}