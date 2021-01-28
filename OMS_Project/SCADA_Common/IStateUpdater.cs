namespace SCADA_Common
{
    /// <summary>
    /// Interface containing logic for updateing connection state and logging.
    /// </summary>
    public interface IStateUpdater
	{
        /// <summary>
        /// Updates the connection state.
        /// </summary>
        /// <param name="currentConnectionState">The new connection state.</param>
		void UpdateConnectionState(ConnectionState currentConnectionState);

        /// <summary>
        /// Logs the message.
        /// </summary>
        /// <param name="message">The message that should be logged.</param>
		void LogMessage(string message);
	}
}