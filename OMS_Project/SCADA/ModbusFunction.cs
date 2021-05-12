using Common.SCADA;
using System;
using System.Collections.Generic;
using System.Net;

namespace SCADA
{
	public abstract class ModbusFunction : IModbusFunction
	{
		public abstract ModbusCommandParameters CommandParameters { get; }

		public abstract byte[] PackRequest();
		public abstract Dictionary<Tuple<EPointType, ushort>, ushort> ParseResponse(byte[] response, out EModbusExceptionCode exceptionCode);

		protected EModbusExceptionCode GetExceptionCode(byte code)
		{
			return (EModbusExceptionCode)(code - 0x80);
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