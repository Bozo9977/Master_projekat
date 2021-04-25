using Common.PubSub;
using System;
using System.Collections.Generic;
using System.Threading;

namespace GUI
{
	public class Measurements
	{
		Dictionary<long, float> analogValues;
		Dictionary<long, int> discreteValues;
		ReaderWriterLockSlim rwLock;

		public Measurements()
		{
			analogValues = new Dictionary<long, float>();
			discreteValues = new Dictionary<long, int>();
			rwLock = new ReaderWriterLockSlim();
		}

		public float GetAnalogValue(long gid)
		{
			rwLock.EnterReadLock();

			try
			{
				float value;
				analogValues.TryGetValue(gid, out value);
				return value;
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public int GetDiscreteValue(long gid)
		{
			rwLock.EnterReadLock();

			try
			{
				int value;
				discreteValues.TryGetValue(gid, out value);
				return value;
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public bool Update(MeasurementValuesChanged message)
		{
			rwLock.EnterWriteLock();

			try
			{
				foreach(Tuple<long, float> a in message.AnalogValues)
				{
					analogValues[a.Item1] = a.Item2;
				}

				foreach(Tuple<long, int> d in message.DiscreteValues)
				{
					discreteValues[d.Item1] = d.Item2;
				}

				return true;
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}
	}
}
