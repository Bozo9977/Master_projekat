using Common.PubSub;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	public class Measurements
	{
		Dictionary<long, float> analogValues;
		Dictionary<long, int> digitalValues;

		public Measurements()
		{
			analogValues = new Dictionary<long, float>();
			digitalValues = new Dictionary<long, int>();
		}

		public float GetAnalogValue(long gid)
		{
			return 0;
		}

		public int GetDigitalValue(long gid)
		{
			return 0;
		}

		public bool Update(MeasurementValuesChanged message)
		{
			return true;
		}
	}
}
