using SCADA_Common;

namespace SCADA_Client.ViewModel.PointViewModels
{
    public abstract class AnalogBase : BasePointItem, IAnalogPoint 
	{
		private double eguValue;

		public AnalogBase(IConfigItem c, IProcessingManager processingManager, IStateUpdater stateUpdater, IConfiguration configuration, int i)
			: base(c, processingManager, stateUpdater, configuration, i)
		{
		}

		public double EguValue
		{
			get
			{
				return eguValue;
			}

			set
			{
				eguValue = value;
				OnPropertyChanged("DisplayValue");
			}
		}

		public override string DisplayValue
		{
			get
			{
				return EguValue.ToString();
			}
		}

        protected override bool WriteCommand_CanExecute(object obj)
        {
            return false;
        }
    }
}
