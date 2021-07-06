using Common.GDA;
using Common.DataModel;
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
		Dictionary<DMSType, int> GetCounters();
		bool PersistDelta(List<IdentifiedObject> inserted, List<IdentifiedObject> updatedNew, List<IdentifiedObject> deleted, Dictionary<DMSType, int> newCounters);
		bool RollbackDelta(List<IdentifiedObject> inserted, List<IdentifiedObject> updatedOld, List<IdentifiedObject> deleted, Dictionary<DMSType, int> oldCounters);
	}
}
