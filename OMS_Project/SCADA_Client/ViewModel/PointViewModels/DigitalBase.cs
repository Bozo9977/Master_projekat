using SCADA_Common;

namespace SCADA_Client.ViewModel.PointViewModels
{
    public abstract class DigitalBase : BasePointItem, IDigitalPoint
    {
		private DState state;

		public DigitalBase(IConfigItem c, IProcessingManager processingManager, IStateUpdater stateUpdater, IConfiguration configuration, int i) 
			: base(c, processingManager, stateUpdater, configuration, i)
		{
        }

		public DState State
		{
			get
			{
				return state;
			}

			set
			{
				state = value;
				OnPropertyChanged("State");
				OnPropertyChanged("DisplayValue");
			}
		}

		public override string DisplayValue
		{
			get
			{
				return State.ToString();
			}
		}

        protected override bool WriteCommand_CanExecute(object obj)
        {
            return false;
        }
    }
}
