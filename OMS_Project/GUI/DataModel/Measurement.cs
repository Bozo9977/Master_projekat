using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.DataModel
{
    public class Measurement : IdentifiedObject
    {
		public int BaseAddress { get; protected set; }
		
		public long PowerSystemResource { get; protected set; }
		public long Terminal { get; protected set; }

		public Measurement() { }

		public Measurement(Measurement m) : base(m)
		{
			BaseAddress = m.BaseAddress;
			PowerSystemResource = m.PowerSystemResource;
			Terminal = m.Terminal;
		}

		public Measurement(List<Property> props, ModelCode code) : base(props, code)
		{
			foreach (var prop in props)
			{
				switch (prop.Id)
				{
					case ModelCode.MEASUREMENT_BASEADDRESS:
						BaseAddress = ((Int32Property)prop).Value;
						break;
					default:
						break;
				}
			}
		}
	}
}
