using Common.PubSub;
using System.Collections.Concurrent;
using System.Collections.Generic;

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
			foreach(KeyValuePair<long, float> a in message.AnalogInputs)
			{
				analogInputs[a.Key] = a.Value;
			}

			foreach(KeyValuePair<long, float> a in message.AnalogOutputs)
			{
				analogOutputs[a.Key] = a.Value;
			}

			foreach(KeyValuePair<long, int> d in message.DiscreteInputs)
			{
				discreteInputs[d.Key] = d.Value;
			}

			foreach(KeyValuePair<long, int> d in message.DiscreteOutputs)
			{
				discreteOutputs[d.Key] = d.Value;
			}
		}
	}
}
