using Common.SCADA;
using System;

namespace SCADASim
{
	public class WriteSingleRegisterRequest : ModbusRequest
	{
		public ushort Address { get; private set; }
		public ushort Value { get; private set; }

		public WriteSingleRegisterRequest(byte[] data, ModbusTCPServer server) : base(data, server)
		{
			Address = UnpackUInt16(data, 8);
			Value = UnpackUInt16(data, 10);
		}

		public override byte[] PackResponse()
		{
			try
			{
				server.SetHoldingRegister(Address, unchecked((short)Value));
			}
			catch(Exception e)
			{
				return PackExceptionResponse(EModbusExceptionCode.ILLEGAL_DATA_ADDRESS);
			}

			byte[] response = new byte[12];
			PackHeader(6, response);
			response[7] = (byte)FunctionCode;
			PackUInt16(Address, response, 8);
			PackUInt16(Value, response, 10);

			return response;
		}
	}
}