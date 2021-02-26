using Common.GDA;
using NMS.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS
{
	interface INMSDatabase
	{
		List<IdentifiedObject> GetList(DMSType type);
		bool PersistDelta(List<IdentifiedObject> inserted, List<IdentifiedObject> updatedNew, List<IdentifiedObject> deleted);
		bool RollbackDelta(List<IdentifiedObject> inserted, List<IdentifiedObject> updatedOld, List<IdentifiedObject> deleted);
	}
}
