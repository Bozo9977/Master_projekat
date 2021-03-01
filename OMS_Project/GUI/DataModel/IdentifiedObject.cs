using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.DataModel
{
    public class IdentifiedObject
    {
		public long GID { get; protected set; }
		public string Name { get; protected set; }
		public string MRID { get; protected set; }

		public IdentifiedObject() { }

		public IdentifiedObject(IdentifiedObject io)
		{
			GID = io.GID;
			Name = io.Name;
			MRID = io.MRID;
		}

		public IdentifiedObject(List<Property> props, ModelCode code)
        {
			foreach(var prop in props)
            {
				switch (prop.Id)
				{
					case ModelCode.IDENTIFIEDOBJECT_GID:
						GID = ((Int64Property)prop).Value;
						break;

					case ModelCode.IDENTIFIEDOBJECT_NAME:
						Name = ((StringProperty)prop).Value;
						break;

					case ModelCode.IDENTIFIEDOBJECT_MRID:
						MRID = ((StringProperty)prop).Value;
						break;
						
				}
			}
        }
	}
}
