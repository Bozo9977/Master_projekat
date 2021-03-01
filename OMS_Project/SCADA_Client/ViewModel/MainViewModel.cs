using Messages.Commands;
using Messages.Events;
using Modbus.Connection;
using NServiceBus;
using ProcessingModule;
using SCADA_Client.Configuration;
using SCADA_Client.PubSubCode;
using SCADA_Client.ViewModel.PointViewModels;
using SCADA_Common;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace SCADA_Client.ViewModel
{
	internal class MainViewModel : ViewModelBase, IDisposable, IStateUpdater, IStorage
	{
		public ObservableCollection<BasePointItem> Points { get; set; }

		#region Fields

		private object lockObject = new object();
		private Thread timerWorker;
		private ConnectionState connectionState;
		private Acquisitor acquisitor;
		private AutoResetEvent acquisitionTrigger = new AutoResetEvent(false);
		private TimeSpan elapsedTime = new TimeSpan();
		private Dispatcher dispather = Dispatcher.CurrentDispatcher;
		private string logText;
		private StringBuilder logBuilder;
		private DateTime currentTime;
		private IFunctionExecutor commandExecutor;
		private IAutomationManager automationManager;
		private bool timerThreadStopSignal = true;
		private bool disposed = false;
		IConfiguration configuration;
		private IProcessingManager processingManager = null;
		private static IEndpointInstance endpointInstance;
		#endregion Fields

		Dictionary<int, IPoint> pointsCache = new Dictionary<int, IPoint>();

		#region Properties

		public DateTime CurrentTime
		{
			get
			{
				return currentTime;
			}

			set
			{
				currentTime = value;
				OnPropertyChanged("CurrentTime");
			}
		}

		public ConnectionState ConnectionState
		{
			get
			{
				return connectionState;
			}

			set
			{
				connectionState = value;
				if (connectionState == ConnectionState.CONNECTED)
				{
					automationManager.Start(configuration.DelayBetweenCommands);
				}
				OnPropertyChanged("ConnectionState");
			}
		}

		public string LogText
		{
			get
			{
				return logText;
			}

			set
			{
				logText = value;
				OnPropertyChanged("LogText");
			}
		}

		public TimeSpan ElapsedTime
		{
			get
			{
				return elapsedTime;
			}

			set
			{
				elapsedTime = value;
				OnPropertyChanged("ElapsedTime");
			}
		}

		#endregion Properties

		private CancellationTokenSource src = new CancellationTokenSource();

		public MainViewModel()
		{
			configuration = new ConfigReader();
			commandExecutor = new FunctionExecutor(this, configuration);
			this.processingManager = new ProcessingManager(this, commandExecutor);
			this.acquisitor = new Acquisitor(acquisitionTrigger, this.processingManager, this, configuration);
			this.automationManager = new AutomationManager(this, processingManager);
			InitializePointCollection();
			InitializeAndStartThreads();
			PeriodicPublishPointInfo(TimeSpan.FromSeconds(3), src.Token).ConfigureAwait(false);
			logBuilder = new StringBuilder();
			ConnectionState = ConnectionState.DISCONNECTED;
			Thread.CurrentThread.Name = "Main Thread";

		}

		#region Private methods

		private void InitializePointCollection()
		{
			//pubsub
			AnalogUpdated analogUpdated;
			DiscreteUpdated discreteUpdated;

			Points = new ObservableCollection<BasePointItem>();
			foreach (var c in configuration.GetConfigurationItems())
			{
				for (int i = 0; i < c.NumberOfRegisters; i++)
				{
					BasePointItem pi = CreatePoint(c, i, this.processingManager);
					if (pi != null)
					{
						Points.Add(pi);
						pointsCache.Add(pi.PointId, pi as IPoint);

						processingManager.InitializePoint(pi.Type, pi.Address, pi.RawValue);
					}
				}
			}
		}

		private async Task PeriodicPublishPointInfo(TimeSpan interval, CancellationToken cancellationToken)
		{
			while (true)
			{
				await PublishPointInfo();
				await Task.Delay(interval, cancellationToken);
			}
		}


		private async Task PublishPointInfo()
        {
			using (EndpointHandler eH = new EndpointHandler())
			{
				AnalogUpdated analogUpdated;
				DiscreteUpdated discreteUpdated;
				await eH.AsyncEndpointCreate();

				
				foreach (var pi in Points)
				{
					if (pi.Type == PointType.ANALOG_INPUT || pi.Type == PointType.ANALOG_OUTPUT)
					{
						analogUpdated = new AnalogUpdated() { Name = pi.Name, Value = pi.RawValue, Address = pi.Address };

						IPoint temp = pointsCache[pi.PointId];
						if (temp != null)
							analogUpdated.Value = temp.RawValue;

						await eH.EndpointInstance.Publish(analogUpdated);
					}

					if (pi.Type == PointType.DIGITAL_INPUT || pi.Type == PointType.DIGITAL_OUTPUT)
					{
						discreteUpdated = new DiscreteUpdated() { Name = pi.Name, Value = (short)pi.RawValue, Address = pi.Address };

						IPoint temp = pointsCache[pi.PointId];
						if (temp != null)
							discreteUpdated.Value = (short)temp.RawValue;

						await eH.EndpointInstance.Publish(discreteUpdated);
					}
				}
			
			}
		}

		private BasePointItem CreatePoint(IConfigItem c, int i, IProcessingManager processingManager)
		{
			switch (c.RegistryType)
			{
				case PointType.DIGITAL_INPUT:
					return new DigitalInput(c, processingManager, this, configuration, i);

				case PointType.DIGITAL_OUTPUT:
					return new DigitalOutput(c, processingManager, this, configuration, i);

				case PointType.ANALOG_INPUT:
					return new AnalogInput(c, processingManager, this, configuration, i);

				case PointType.ANALOG_OUTPUT:
					return new AnalogOutput(c, processingManager, this, configuration, i);

				default:
					return null;
			}
		}

		private void InitializeAndStartThreads()
		{
			InitializeTimerThread();
			StartTimerThread();
		}

		private void InitializeTimerThread()
		{
			timerWorker = new Thread(TimerWorker_DoWork);
			timerWorker.Name = "Timer Thread";
		}

		private void StartTimerThread()
		{
			timerWorker.Start();
		}

		/// <summary>
		/// Timer thread:
		///		Refreshes timers on UI and signalizes to acquisition thread that one second has elapsed
		/// </summary>
		private void TimerWorker_DoWork()
		{
			while (timerThreadStopSignal)
			{
				if (disposed)
					return;

				CurrentTime = DateTime.Now;
				ElapsedTime = ElapsedTime.Add(new TimeSpan(0, 0, 1));
				acquisitionTrigger.Set();
				Thread.Sleep(1000);
			}
		}

		#endregion Private methods

		#region IStateUpdater implementation

		public void UpdateConnectionState(ConnectionState currentConnectionState)
		{
			dispather.Invoke((Action)(() =>
			{
				ConnectionState = currentConnectionState;
			}));
		}

		public void LogMessage(string message)
		{
			if (disposed)
				return;

			string threadName = Thread.CurrentThread.Name;

			dispather.Invoke((Action)(() =>
			{
				lock (lockObject)
				{
					logBuilder.Append($"{DateTime.Now} [{threadName}]: {message}{Environment.NewLine}");
					LogText = logBuilder.ToString();
				}
			}));
		}

		#endregion IStateUpdater implementation

		public void Dispose()
		{
			disposed = true;
			timerThreadStopSignal = false;
			src.Cancel();
			(commandExecutor as IDisposable).Dispose();
			this.acquisitor.Dispose();
			acquisitionTrigger.Dispose();
			automationManager.Stop();
		}

		public List<IPoint> GetPoints(List<PointIdentifier> pointIds)
		{
			List<IPoint> retVal = new List<IPoint>(pointIds.Count);
			foreach (var pid in pointIds)
			{
				int id = PointIdentifierHelper.GetNewPointId(pid);
				IPoint p = null;
				if (pointsCache.TryGetValue(id, out p))
				{
					retVal.Add(p);
				}
			}
			return retVal;
		}
	}
}
