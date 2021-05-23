using Common.PubSub;
using System;
using System.Collections.Concurrent;

namespace GUI
{
	public class Measurements
	{
		ConcurrentDictionary<long, float> analogInputs;
		ConcurrentDictionary<long, float> analogOutputs;
		ConcurrentDictionary<long, int> discreteInputs;
		ConcurrentDictionary<long, int> discreteOutputs;

		public Measurements()
		{
			analogInputs = new ConcurrentDictionary<long, float>();
			analogOutputs = new ConcurrentDictionary<long, float>();
			discreteInputs = new ConcurrentDictionary<long, int>();
			discreteOutputs = new ConcurrentDictionary<long, int>();
		}

		public bool GetAnalogInput(long gid, out float value)
		{
			return analogInputs.TryGetValue(gid, out value);
		}

		public bool GetAnalogOutput(long gid, out float value)
		{
			return analogOutputs.TryGetValue(gid, out value);
		}

		public bool GetDiscreteInput(long gid, out int value)
		{
			return discreteInputs.TryGetValue(gid, out value);
		}

		public bool GetDiscreteOutput(long gid, out int value)
		{
			return discreteOutputs.TryGetValue(gid, out value);
		}

		public void Update(MeasurementValuesChanged message)
		{
			foreach(Tuple<long, float> a in message.AnalogInputs)
			{
				analogInputs[a.Item1] = a.Item2;
			}

			foreach(Tuple<long, float> a in message.AnalogOutputs)
			{
				analogOutputs[a.Item1] = a.Item2;
			}

			foreach(Tuple<long, int> d in message.DiscreteInputs)
			{
				discreteInputs[d.Item1] = d.Item2;
			}

			foreach(Tuple<long, int> d in message.DiscreteOutputs)
			{
				discreteOutputs[d.Item1] = d.Item2;
			}
		}
	}
}
