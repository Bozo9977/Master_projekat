using SCADA_Client.Utils;
using SCADA_Common;
using System;
using System.ComponentModel;
using System.Windows.Threading;

namespace SCADA_Client.ViewModel.PointViewModels
{
    public abstract class BasePointItem : ViewModelBase, IDataErrorInfo
	{
		protected PointType type;
		protected ushort address;
		private DateTime timestamp = DateTime.Now;
		private string name = string.Empty;
		private ushort rawValue;
		private double commandedValue;
		protected AlarmType alarm;

		protected IProcessingManager processingManager;
		protected IConfiguration configuration;

		protected Dispatcher dispatcher = Dispatcher.CurrentDispatcher;

		protected IStateUpdater stateUpdater;
        protected IConfigItem configItem;

        int pointId;
		/// <summary>
		/// Command that is executed when write button is clicked on control window;
		/// Command should create write command parameters and provide it to FunctionFactory
		/// </summary>
		public RelayCommand WriteCommand { get; set; }

		/// <summary>
		/// Command that is executed when read button is clicked on control window;
		/// Command should create read command parameters and provide it to FunctionFactory
		/// </summary>
		public RelayCommand ReadCommand { get; set; }

		public BasePointItem(IConfigItem c, IProcessingManager processingManager, IStateUpdater stateUpdater, IConfiguration configuration, int i)
		{
			this.configItem = c;
			this.processingManager = processingManager;
			this.stateUpdater = stateUpdater;
			this.configuration = configuration;

			this.type = c.RegistryType;
			this.address = (ushort)(c.StartAddress+i);
			this.name = $"{configItem.Description} [{i}]";
			this.rawValue = configItem.DefaultValue;
			this.pointId = PointIdentifierHelper.GetNewPointId(new PointIdentifier(this.type, this.address));

			WriteCommand = new RelayCommand(WriteCommand_Execute, WriteCommand_CanExecute);
			ReadCommand = new RelayCommand(ReadCommand_Execute);
		}

        protected abstract bool WriteCommand_CanExecute(object obj);

        /// <summary>
        /// Method that is executed when write button is clicked on control window;
        /// Method should create write command parameters and provide it to FunctionFactory
        /// </summary>
        /// <param name="obj">Not used</param>
        protected abstract void WriteCommand_Execute(object obj);

		/// <summary>
		/// Method that is executed when read button is clicked on control window;
		/// Method should create read command parameters and provide it to FunctionFactory
		/// </summary>
		/// <param name="obj">Not used</param>
		private void ReadCommand_Execute(object obj)
		{
			try
			{
                this.processingManager.ExecuteReadCommand(configItem, configuration.GetTransactionId(), configuration.UnitAddress, address, 1);
			}
			catch (Exception ex)
			{
				string message = $"{ex.TargetSite.ReflectedType.Name}.{ex.TargetSite.Name}: {ex.Message}";
				this.stateUpdater.LogMessage(message);
			}
		}

		#region Properties

		public PointType Type
		{
			get
			{
				return type;
			}

			set
			{
				type = value;
				OnPropertyChanged("Type");
			}
		}

		/// <summary>
		/// Address of point on MdbSim Simulator
		/// </summary>
		public ushort Address
		{
			get
			{
				return address;
			}

			set
			{
				address = value;
				OnPropertyChanged("Address");
			}
		}

		public DateTime Timestamp
		{
			get
			{
				return timestamp;
			}

			set
			{
				timestamp = value;
				OnPropertyChanged("Timestamp");
			}
		}

		public string Name
		{
			get
			{
				return name;
			}

			set
			{
				name = value;
			}
		}

		public virtual string DisplayValue
		{
			get
			{
				return string.Empty;
			}
		}

		/// <summary>
		/// Value that is sent on MdbSim simulator
		/// </summary>
		public double CommandedValue
		{
			get
			{
				return commandedValue;
			}

			set
			{
				commandedValue = value;
				OnPropertyChanged("CommandedValue");
			}
		}

		/// <summary>
		/// Raw value, read from MdbSim
		/// </summary>
		public virtual ushort RawValue
		{
			get
			{
				return rawValue;
			}
			set
			{
				rawValue = value;
				OnPropertyChanged("RawValue");
			}
		}

        public IConfigItem ConfigItem
        {
            get
            {
                return configItem;
            }
        }

        #endregion Properties

        #region Input validation

        public string Error
		{
			get
			{
				return string.Empty;
			}
		}

		public AlarmType Alarm
		{
			get
			{
				return alarm;
			}

            set
            {
                alarm = value;
                OnPropertyChanged("Alarm");
            }
		}

		public int PointId
		{
			get
			{
				return pointId;
			}
		}

        public string this[string columnName]
		{
			get
			{
				string message = string.Empty;
				if (columnName == "CommandedValue")
				{
					if (commandedValue > configItem.MaxValue)
					{
						message = $"Entered value cannot be greater than {configItem.MaxValue}.";
					}
					if (commandedValue < configItem.MinValue)
					{
						message = $"Entered value cannot be lower than {configItem.MinValue}.";
					}
				}
				return message;
			}
		}

		#endregion Input validation
	}
}