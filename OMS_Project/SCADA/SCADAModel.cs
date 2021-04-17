using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCADA
{
	public class AnalogModel
	{
		public Analog Analog { get; private set; }
		public float Value { get; set; }

		public AnalogModel(Analog a)
		{
			Analog = a;
		}
	}

	public class DiscreteModel
	{
		public Discrete Discrete { get; private set; }
		public int Value { get; set; }

		public DiscreteModel(Discrete d)
		{
			Discrete = d;
		}
	}

	public class SCADAModel
	{
		static SCADAModel instance;

		Dictionary<long, AnalogModel> analogs;
		Dictionary<long, DiscreteModel> discretes;
		Dictionary<int, long> byAddress;
		ReaderWriterLockSlim valuesLock;

		public static SCADAModel Instance
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

		public SCADAModel(SCADAModelDownload download)
		{
			byAddress = new Dictionary<int, long>();

			foreach(KeyValuePair<long, IdentifiedObject> analogPair in download.Containers[DMSType.Analog])
			{
				Analog analog = (Analog)analogPair.Value;

				if(byAddress.ContainsKey(analog.BaseAddress))
				{
					continue;
				}

				analogs.Add(analogPair.Key, new AnalogModel(analog));
				byAddress.Add(analog.BaseAddress, analogPair.Key);
			}

			foreach(KeyValuePair<long, IdentifiedObject> discretePair in download.Containers[DMSType.Discrete])
			{
				Discrete discrete = (Discrete)discretePair.Value;

				if(byAddress.ContainsKey(discrete.BaseAddress))
				{
					continue;
				}

				discretes.Add(discretePair.Key, new DiscreteModel(discrete));
				byAddress.Add(discrete.BaseAddress, discretePair.Key);
			}

			valuesLock = new ReaderWriterLockSlim();
		}

		public SCADAModel(SCADAModelEFDatabase db)
		{
			byAddress = new Dictionary<int, long>();

			foreach(Analog analog in db.GetList(DMSType.Analog))
			{
				analogs.Add(analog.GID, new AnalogModel(analog));

				if(!byAddress.ContainsKey(analog.BaseAddress))
				{
					byAddress.Add(analog.BaseAddress, analog.GID);
				}
			}

			foreach(Discrete discrete in db.GetList(DMSType.Discrete))
			{
				discretes.Add(discrete.GID, new DiscreteModel(discrete));

				if(!byAddress.ContainsKey(discrete.BaseAddress))
				{
					byAddress.Add(discrete.BaseAddress, discrete.GID);
				}
			}

			valuesLock = new ReaderWriterLockSlim();
		}

		public List<float> GetAnalog(List<long> gids)
		{
			if(gids == null)
			{
				return null;
			}

			List<float> values = new List<float>(gids.Count);
			valuesLock.EnterReadLock();

			try
			{
				foreach(long gid in gids)
				{
					AnalogModel a;
					float value = 0;

					if(analogs.TryGetValue(gid, out a))
					{
						value = a.Value;
					}

					values.Add(value);
				}
			}
			finally
			{
				valuesLock.ExitReadLock();
			}

			return values;
		}

		public List<int> GetDiscrete(List<long> gids)
		{
			if(gids == null)
			{
				return null;
			}

			List<int> values = new List<int>(gids.Count);
			valuesLock.EnterReadLock();

			try
			{
				foreach(long gid in gids)
				{
					DiscreteModel d;
					int value = 0;

					if(discretes.TryGetValue(gid, out d))
					{
						value = d.Value;
					}

					values.Add(value);
				}
			}
			finally
			{
				valuesLock.ExitReadLock();
			}

			return values;
		}

		private int Min(int x, int y)
		{
			return x < y ? x : y;
		}

		public void SetValues(List<long> analogGids, List<float> analogValues, List<long> discreteGids, List<int> discreteValues)
		{
			valuesLock.EnterWriteLock();

			try
			{
				if(analogGids != null && analogValues != null)
				{
					for(int i = 0; i < Min(analogGids.Count, analogValues.Count); ++i)
					{
						AnalogModel a;

						if(analogs.TryGetValue(analogGids[i], out a))
						{
							a.Value = analogValues[i];
						}
					}
				}

				if(discreteGids != null && discreteValues != null)
				{
					for(int i = 0; i < Min(discreteGids.Count, discreteValues.Count); ++i)
					{
						DiscreteModel d;

						if(discretes.TryGetValue(discreteGids[i], out d))
						{
							d.Value = discreteValues[i];
						}
					}
				}
			}
			finally
			{
				valuesLock.ExitWriteLock();
			}
		}
	}
}
