using System;
using System.Collections.Generic;

namespace SCADA_Common
{
    /// <summary>
    /// Interface containing logic for parsing and packing modbus functions/requests.
    /// </summary>
    public interface IModbusFunction
	{
        /// <summary>
        /// Parses the modbus response.
        /// </summary>
        /// <param name="receivedBytes">The received modbus response.</param>
        /// <returns>Parsed values grouped by type and address.</returns>
		Dictionary<Tuple<PointType, ushort>, ushort> ParseResponse(byte[] receivedBytes);

        /// <summary>
        /// Packs the modbus request.
        /// </summary>
        /// <returns>The packet modbus request.</returns>
		byte[] PackRequest();
	}
}
