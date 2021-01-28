namespace SCADA_Common
{
    /// <summary>
    /// Interface containing logic for processing points and executing commands.
    /// </summary>
    public interface IProcessingManager
    {
        /// <summary>
        /// Initializes and sets the default value of the point.
        /// </summary>
        /// <param name="type">Type of the point.</param>
        /// <param name="pointAddress">Address of the point.</param>
        /// <param name="defaultValue">Default value of the point.</param>
        void InitializePoint(PointType type, ushort pointAddress, ushort defaultValue);

        /// <summary>
        /// Executes a write command.
        /// </summary>
        /// <param name="configItem">The configuration item.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="remoteUnitAddress">The remote unit address.</param>
        /// <param name="pointAddress">The point address.</param>
        /// <param name="value">The value.</param>
        void ExecuteWriteCommand(IConfigItem configItem, ushort transactionId, byte remoteUnitAddress, ushort pointAddress, int value);

        /// <summary>
        /// Executes a read command.
        /// </summary>
        /// <param name="configItem">The configuration item.</param>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="remoteUnitAddress">The remote unit address.</param>
        /// <param name="startAddress">The start address.</param>
        /// <param name="numberOfPoints">The number of points that should be read.</param>
        void ExecuteReadCommand(IConfigItem configItem, ushort transactionId, byte remoteUnitAddress, ushort startAddress, ushort numberOfPoints);
    }
}
