using SCADA_Common;
using System;

namespace SCADA_Client.ViewModel.PointViewModels
{
    internal class DigitalOutput : DigitalBase
	{

		public DigitalOutput(IConfigItem c, IProcessingManager processingManager, IStateUpdater stateUpdater, IConfiguration configuration, int i)
			: base(c, processingManager, stateUpdater, configuration, i)
		{
		}

		protected override bool WriteCommand_CanExecute(object obj)
		{
			return !(CommandedValue < 0 || CommandedValue > 1);
		}

        protected override void WriteCommand_Execute(object obj)
        {
            try
            {
                this.processingManager.ExecuteWriteCommand(ConfigItem, configuration.GetTransactionId(), configuration.UnitAddress, address, (int)CommandedValue);
            }
            catch (Exception ex)
            {
                string message = $"{ex.TargetSite.ReflectedType.Name}.{ex.TargetSite.Name}: {ex.Message}";
                this.stateUpdater.LogMessage(message);
            }
        }
    }
}