namespace SCADA_Common
{
    /// <summary>
    /// Interface containing methods for starting and stopping the automation manager.
    /// </summary>
	public interface IAutomationManager
	{
        /// <summary>
        /// Starts the automation manager.
        /// </summary>
        /// <param name="delayBetWweenCommands">The delay between commands (in seconds).</param>
		void Start(int delayBetWweenCommands);

        /// <summary>
        /// Stops the autimation manager.
        /// </summary>
		void Stop();
	}
}
