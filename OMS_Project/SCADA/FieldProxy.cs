using Common.DataModel;
using Common.PubSub;
using Common.WCF;
using System;
using System.Collections.Generic;
using System.Threading;

namespace SCADA
{
	public class FieldProxy
	{
		static FieldProxy instance;

		ReaderWriterLockSlim rwLock;
		ManualResetEvent stopSignal;
		List<long> analogs;
		List<long> discretes;
		Thread simThread;
		SCADAModel model;

		public static FieldProxy Instance
		{
			get
			{
				return instance;
			}
			set
			{
				Interlocked.Exchange(ref instance, value);
			}
		}

		public FieldProxy()
		{
			rwLock = new ReaderWriterLockSlim();
			stopSignal = new ManualResetEvent(false);
		}

		public bool UpdateModel()
		{
			rwLock.EnterWriteLock();

			try
			{
				if(simThread != null)
				{
					stopSignal.Set();
					simThread.Join();
				}

				model = SCADAModel.Instance;

				if(model == null)
					return false;

				this.analogs = model.GetAnalogGIDs();
				this.discretes = model.GetDiscreteGIDs();

				stopSignal.Reset();
				simThread = new Thread(Simulation);
				simThread.Start();

				return true;
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		public void Stop()
		{
			if(simThread != null)
			{
				stopSignal.Set();
				simThread.Join();
			}
		}

		void Simulation()
		{
			Random r = new Random();

			while(!stopSignal.WaitOne(1000))
			{
				List<Tuple<long, float>> analogNewValues;
				List<Tuple<long, int>> discreteNewValues;

				rwLock.EnterReadLock();

				try
				{
					analogNewValues = new List<Tuple<long, float>>(analogs.Count);
					discreteNewValues = new List<Tuple<long, int>>(discretes.Count);

					foreach(long gid in analogs)
					{
						Analog a = model.GetAnalog(gid);
						analogNewValues.Add(new Tuple<long, float>(gid, a.NormalValue + ((float)r.NextDouble() * 10 - 5)));
					}

					foreach(long gid in discretes)
					{
						Discrete d = model.GetDiscrete(gid);
						discreteNewValues.Add(new Tuple<long, int>(gid, d.NormalValue + (r.Next(11) - 5)));
					}
				}
				finally
				{
					rwLock.ExitReadLock();
				}

				Client<IPublishing> pubClient = new Client<IPublishing>("publishingEndpoint");
				pubClient.Connect();

				pubClient.Call<bool>(pub =>
				{
					pub.Publish(new MeasurementValuesChanged() { AnalogValues = analogNewValues, DiscreteValues = discreteNewValues });
					return true;
				}, out _);

				pubClient.Disconnect();
			}
		}
	}
}