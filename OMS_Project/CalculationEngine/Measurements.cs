using Common.GDA;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace CalculationEngine
{
	public class Measurements
	{
		static volatile Measurements instance;

		public static Measurements Instance
		{
			get
			{
				return instance;
			}
			set
			{
				instance = value;
			}
		}

		ConcurrentDictionary<long, float> analogInputs;
		ConcurrentDictionary<long, int> discreteInputs;
		HashSet<long> gidsOfInterest;
		ReaderWriterLockSlim rwLock;

		public Measurements()
		{
			analogInputs = new ConcurrentDictionary<long, float>();
			discreteInputs = new ConcurrentDictionary<long, int>();
			gidsOfInterest = new HashSet<long>();
			rwLock = new ReaderWriterLockSlim();
		}

		public bool TryGetAnalog(long gid, out float value)
		{
			return analogInputs.TryGetValue(gid, out value);
		}

		public bool TryGetDiscrete(long gid, out int value)
		{
			return discreteInputs.TryGetValue(gid, out value);
		}

		public void SetAnalogs(List<Tuple<long, float>> analogs)
		{
			if(analogs == null)
				return;

			rwLock.EnterReadLock();
			try
			{
				for(int i = 0; i < analogs.Count; ++i)
				{
					Tuple<long, float> analog = analogs[i];

					if(gidsOfInterest.Contains(analog.Item1))
					{
						analogInputs[analog.Item1] = analog.Item2;
					}
				}
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public void SetDiscretes(List<Tuple<long, int>> discretes)
		{
			if(discretes == null)
				return;

			rwLock.EnterReadLock();
			try
			{
				for(int i = 0; i < discretes.Count; ++i)
				{
					Tuple<long, int> discrete = discretes[i];

					if(gidsOfInterest.Contains(discrete.Item1))
					{
						discreteInputs[discrete.Item1] = discrete.Item2;
					}
				}
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public void UpdateModel()
		{
			lock(rwLock)
			{
				HashSet<long> oldGids = this.gidsOfInterest;
				TopologyModel tm = TopologyModel.Instance;
				HashSet<long> gidsOfInterest = tm.GetMeasurementGIDsOfInterest();
				rwLock.EnterWriteLock();
				this.gidsOfInterest = gidsOfInterest;
				rwLock.ExitWriteLock();

				foreach(long gid in oldGids)
				{
					if(gidsOfInterest.Contains(gid))
						continue;

					switch(ModelCodeHelper.GetTypeFromGID(gid))
					{
						case DMSType.Analog:
							analogInputs.TryRemove(gid, out _);
							break;

						case DMSType.Discrete:
							discreteInputs.TryRemove(gid, out _);
							break;
					}
				}
			}
		}
	}
}