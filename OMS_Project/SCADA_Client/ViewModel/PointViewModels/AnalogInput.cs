using SCADA_Common;

namespace SCADA_Client.ViewModel.PointViewModels
{
    public class AnalogInput : AnalogBase
    {
        public AnalogInput(IConfigItem c, IProcessingManager processingManager, IStateUpdater stateUpdater, IConfiguration configuration, int i)
            : base(c, processingManager, stateUpdater, configuration, i)
        {
        }

        protected override void WriteCommand_Execute(object obj)
        {
            // Write command is not applicable for input points.
        }
    }
}
