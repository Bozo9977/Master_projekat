using Modbus.FunctionParameters;
using SCADA_Common;
using System;
using System.Collections.Generic;
using System.Net;
using System.Reflection;

namespace Modbus.ModbusFunctions
{
    /// <summary>
    /// Class containing logic for parsing and packing modbus read coil functions/requests.
    /// </summary>
    public class ReadCoilsFunction : ModbusFunction
	{
        /// <summary>
        /// Initializes a new instance of the <see cref="ReadCoilsFunction"/> class.
        /// </summary>
        /// <param name="commandParameters">The modbus command parameters.</param>
		public ReadCoilsFunction(ModbusCommandParameters commandParameters) : base(commandParameters)
		{
			CheckArguments(MethodBase.GetCurrentMethod(), typeof(ModbusReadCommandParameters));
		}

        /// <inheritdoc/>
        public override byte[] PackRequest()
        {
            byte[] paket = new byte[12];

            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.TransactionId)), 0, paket, 0, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.ProtocolId)), 0, paket, 2, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)CommandParameters.Length)), 0, paket, 4, 2);
            paket[6] = CommandParameters.UnitId;
            paket[7] = CommandParameters.FunctionCode;
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)((ModbusReadCommandParameters)CommandParameters).StartAddress)), 0, paket, 8, 2);
            Buffer.BlockCopy(BitConverter.GetBytes(IPAddress.HostToNetworkOrder((short)((ModbusReadCommandParameters)CommandParameters).Quantity)), 0, paket, 10, 2);

            return paket;
        }

        /// <inheritdoc />
        public override Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] response)
        {
            var ret = new Dictionary<Tuple<PointType, ushort>, ushort>();

            if (response[7] == CommandParameters.FunctionCode + 0x80)
            {
                HandeException(response[8]);
            }
            else
            {
                int cnt = 0;
                ushort adresa = ((ModbusReadCommandParameters)CommandParameters).StartAddress;
                ushort value;
                byte maska = 1;
                for (int i = 0; i < response[8]; i++)
                {
                    byte tempByte = response[9 + i];
                    for (int j = 0; j < 8; j++)
                    {
                        value = (ushort)(tempByte & maska);
                        tempByte >>= 1;
                        ret.Add(new Tuple<PointType, ushort>(PointType.DIGITAL_OUTPUT, adresa), value);
                        cnt++;
                        adresa++;
                        if (cnt == ((ModbusReadCommandParameters)CommandParameters).Quantity)
                        {
                            break;
                        }
                    }
                }
            }

            return ret;
        }
	}
}