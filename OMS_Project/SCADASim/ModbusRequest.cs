using Common.SCADA;
using System;
using System.Net;

namespace SCADASim
{
	public class ModbusRequest : IModbusRequest
	{
		protected ModbusTCPServer server;
		public ushort TransactionId { get; private set; }
		public ushort ProtocolId { get; private set; }
		public ushort Length { get; private set; }
		public byte UnitId { get; private set; }
		public EModbusFunctionCode FunctionCode { get; private set; }

		public ModbusRequest(byte[] data, ModbusTCPServer server)
		{
			TransactionId = UnpackUInt16(data, 0);
			ProtocolId = UnpackUInt16(data, 2);
			Length = UnpackUInt16(data, 4);
			UnitId = data[6];
			FunctionCode = (EModbusFunctionCode)data[7];
			this.server = server;
		}

		public virtual byte[] PackResponse()
		{
			return PackExceptionResponse(EModbusExceptionCode.ILLEGAL_FUNCTION);
		}

		protected void PackHeader(ushort byteLength, byte[] output, int offset = 0)
		{
			byte[] length = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(unchecked((short)byteLength)));
			byte[] header = new byte[7] { 0, 0, 0, 0, length[0], length[1], 1 };
			Array.Copy(header, 0, output, offset, header.Length);
		}

		protected byte[] PackExceptionResponse(EModbusExceptionCode exceptionCode)
		{
			byte[] data = new byte[9];
			PackHeader(3, data);
			data[7] = (byte)((byte)FunctionCode + 0x80);
			data[8] = (byte)exceptionCode;
			return data;
		}

		protected ushort UnpackUInt16(byte[] src, int offset)
		{
			return unchecked((ushort)IPAddress.NetworkToHostOrder(BitConverter.ToInt16(src, offset)));
		}

		protected void PackUInt16(ushort value, byte[] dst, int offset)
		{
			byte[] bytes = BitConverter.GetBytes(IPAddress.HostToNetworkOrder(unchecked((short)value)));
			dst[offset] = bytes[0];
			dst[offset + 1] = bytes[1];
		}
	}
}
