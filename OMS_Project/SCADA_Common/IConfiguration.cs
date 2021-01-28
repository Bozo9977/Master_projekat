using System.Collections.Generic;

namespace SCADA_Common
{
    public interface IConfiguration
	{
        /// <summary>
        /// Gets the tcp port of the remote unit.
        /// </summary>
		int TcpPort { get; }

        /// <summary>
        /// Gets unit address of the remote unit.
        /// </summary>
		byte UnitAddress { get; }

        /// <summary>
        /// Gets delay between commands (in seconds).
        /// </summary>
		int DelayBetweenCommands { get; }

        /// <summary>
        /// Gets the transaction identifier for the next request.
        /// </summary>
		ushort GetTransactionId();

        /// <summary>
        /// Gets the configuration items.
        /// </summary>
		List<IConfigItem> GetConfigurationItems();

        /// <summary>
        /// Gets the acquiition interval for the registers based on the point description.
        /// </summary>
        /// <param name="pointDescription">The point description</param>
        /// <returns>The acquisition interval (in seconds).</returns>
		int GetAcquisitionInterval(string pointDescription);

        /// <summary>
        /// Gets the start address of the registers based on the point description.
        /// </summary>
        /// <param name="pointDescription">The point description</param>
        /// <returns>The start address of the points.</returns>
		ushort GetStartAddress(string pointDescription);

        /// <summary>
        /// Gets the number of registers for the registers based on the point description.
        /// </summary>
        /// <param name="pointDescription">The point description</param>
        /// <returns>The number of registers.</returns>
        ushort GetNumberOfRegisters(string pointDescription);
	}
}
