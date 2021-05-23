using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Threading;

namespace CalculationEngine
{
	public class TopologyModel
	{
		static volatile TopologyModel instance;

		public static TopologyModel Instance
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

		Dictionary<DMSType, Dictionary<long, IdentifiedObject>> containers;
		ReaderWriterLockSlim rwLock;

		List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> lineEnergization;

		public TopologyModel()
		{
			DMSType[] types = ModelResourcesDesc.TypeIdsInInsertOrder;
			containers = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>(types.Length);

			foreach(DMSType t in types)
				containers.Add(t, new Dictionary<long, IdentifiedObject>());

			rwLock = new ReaderWriterLockSlim();
		}

		public TopologyModel(TopologyModel tm)
		{
			tm.rwLock.EnterReadLock();

			try
			{
				containers = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>(tm.containers.Count);

				foreach(KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> container in tm.containers)
					containers.Add(container.Key, new Dictionary<long, IdentifiedObject>(container.Value));

				rwLock = new ReaderWriterLockSlim();
			}
			finally
			{
				tm.rwLock.ExitReadLock();
			}
		}

		public bool ApplyUpdate(TopologyModelDownload download)
		{
			rwLock.EnterWriteLock();

			try
			{
				foreach(IdentifiedObject io in download.Inserted)
				{
					DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);
					Dictionary<long, IdentifiedObject> container;

					if(!containers.TryGetValue(type, out container))
						continue;

					int oldCount = container.Count;
					container[io.GID] = io;

					if(container.Count != oldCount + 1)
						return false;
				}

				foreach(IdentifiedObject io in download.Updated)
				{
					DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);
					Dictionary<long, IdentifiedObject> container;

					if(!containers.TryGetValue(type, out container))
						continue;

					int oldCount = container.Count;
					container[io.GID] = io;

					if(container.Count != oldCount)
						return false;
				}

				foreach(long gid in download.Deleted)
				{
					DMSType type = ModelCodeHelper.GetTypeFromGID(gid);
					Dictionary<long, IdentifiedObject> container;

					if(!containers.TryGetValue(type, out container))
						continue;

					if(!container.Remove(gid))
						return false;
				}

				return RebuildTopology();
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		public bool RebuildTopology()
		{
			return true;
		}

		public List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> GetLineEnergization()
		{
			return lineEnergization;
		}
	}
}