using Common.SCADA;
using System;

namespace SCADASim
{
	public class ReadInputRegistersRequest : ModbusRequest
	{
		public ushort Address { get; private set; }
		public ushort Quantity { get; private set; }

		public ReadInputRegistersRequest(byte[] data, ModbusTCPServer server) : base(data, server)
		{
			Address = UnpackUInt16(data, 8);
			Quantity = UnpackUInt16(data, 10);
		}

		public override byte[] PackResponse()
		{
			int quantity = Math.Min(Quantity, ushort.MaxValue - Address);
			quantity = Math.Min(quantity, 125);
			short[] values = new short[quantity];

			try
			{
				server.GetInputRegisters(Address, (ushort)quantity, values, 0);
			}
			catch(Exception e)
			{
				return PackExceptionResponse(EModbusExceptionCode.ILLEGAL_DATA_ADDRESS);
			}

			byte[] response = new byte[9 + 2 * quantity];
			PackHeader((ushort)(3 + 2 * quantity), response);
			response[7] = (byte)FunctionCode;
			response[8] = (byte)(2 * quantity);

			int offset = 9;
			foreach(short value in values)
			{
				PackUInt16(unchecked((ushort)value), response, offset);
				offset += 2;
			}

			return response;
		}
	}
}