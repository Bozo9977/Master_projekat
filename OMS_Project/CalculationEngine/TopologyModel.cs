using Common.CalculationEngine;
using Common.DataModel;
using Common.GDA;
using Common.PubSub;
using Common.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
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
		TopologyGraph graph;
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
			}
			finally
			{
				tm.rwLock.ExitReadLock();
			}
			
			rwLock = new ReaderWriterLockSlim();
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

				graph = new TopologyGraph(containers);
				lineEnergization = graph.CalculateLineEnergization();
				return true;
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		public HashSet<long> GetMeasurementGIDsOfInterest()
		{
			rwLock.EnterReadLock();

			try
			{
				HashSet<long> gids = new HashSet<long>();

				foreach(IdentifiedObject d in containers[DMSType.Discrete].Values)
				{
					if(((Discrete)d).MeasurementType == MeasurementType.SwitchState)
					{
						gids.Add(d.GID);
					}
				}

				return gids;
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> GetLineEnergization()
		{
			rwLock.EnterReadLock();
			{
				List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> lineEnergization = this.lineEnergization;
			}
			rwLock.ExitReadLock();

			return lineEnergization;
		}

		public void MeasurementsUpdated(List<Tuple<long, float>> analogInputs, List<Tuple<long, int>> discreteInputs)
		{
			rwLock.EnterUpgradeableReadLock();

			try
			{
				if(graph == null)
					return;

				bool updateTopology = false;

				for(int i = 0; i < discreteInputs.Count; ++i)
				{
					Tuple<long, int> m = discreteInputs[i];
					IdentifiedObject meas;
					if(containers[DMSType.Discrete].TryGetValue(m.Item1, out meas) && ((Discrete)meas).MeasurementType == MeasurementType.SwitchState)
					{
						updateTopology = true;
						break;
					}
				}

				if(!updateTopology)
					return;

				lock(graph)
				{
					List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> lineEnergization = graph.CalculateLineEnergization();

					rwLock.EnterWriteLock();
					{
						this.lineEnergization = lineEnergization;
					}
					rwLock.ExitWriteLock();
				}
			}
			finally
			{
				rwLock.ExitUpgradeableReadLock();
			}

			Client<IPublishing> pubClient = new Client<IPublishing>("publishingEndpoint");
			pubClient.Connect();

			pubClient.Call<bool>(pub =>
			{
				pub.Publish(new TopologyChanged());
				return true;
			}, out _);

			pubClient.Disconnect();
		}
	}
}