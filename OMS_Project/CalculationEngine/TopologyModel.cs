using Common.CalculationEngine;
using Common.DataModel;
using Common.GDA;
using Common.PubSub;
using Common.SCADA;
using Common.WCF;
using System;
using System.Collections.Concurrent;
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
		ConcurrentDictionary<long, float> analogInputs;
		ConcurrentDictionary<long, int> discreteInputs;
		HashSet<long> measurementsOfInterest;

		public TopologyModel()
		{
			DMSType[] types = ModelResourcesDesc.TypeIdsInInsertOrder;
			containers = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>(types.Length);

			foreach(DMSType t in types)
				containers.Add(t, new Dictionary<long, IdentifiedObject>());

			rwLock = new ReaderWriterLockSlim();
			analogInputs = new ConcurrentDictionary<long, float>();
			discreteInputs = new ConcurrentDictionary<long, int>();
			measurementsOfInterest = new HashSet<long>();
		}

		public TopologyModel(TopologyModel tm)
		{
			tm.rwLock.EnterReadLock();

			try
			{
				containers = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>(tm.containers.Count);

				foreach(KeyValuePair<DMSType, Dictionary<long, IdentifiedObject>> container in tm.containers)
					containers.Add(container.Key, new Dictionary<long, IdentifiedObject>(container.Value));

				analogInputs = new ConcurrentDictionary<long, float>();
				discreteInputs = new ConcurrentDictionary<long, int>();
				measurementsOfInterest = new HashSet<long>(tm.measurementsOfInterest);
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

					if(IsMeasurementOfInterest(io))
						measurementsOfInterest.Add(io.GID);
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

					if(IsMeasurementOfInterest(io))
					{
						measurementsOfInterest.Add(io.GID);
					}
					else
					{
						measurementsOfInterest.Remove(io.GID);
					}
				}

				foreach(long gid in download.Deleted)
				{
					DMSType type = ModelCodeHelper.GetTypeFromGID(gid);
					Dictionary<long, IdentifiedObject> container;

					if(!containers.TryGetValue(type, out container))
						continue;

					if(!container.Remove(gid))
						return false;

					measurementsOfInterest.Remove(gid);
				}

				return true;
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		public void DownloadMeasurements(List<long> gids)
		{
			rwLock.EnterUpgradeableReadLock();

			try
			{
				List<long> analogs;
				List<long> discretes;

				if(gids == null)
				{
					analogs = GetAnalogGIDsOfInterest();
					discretes = GetDiscreteGIDsOfInterest();
				}
				else
				{
					analogs = new List<long>();
					discretes = new List<long>();

					for(int i = 0; i < gids.Count; ++i)
					{
						long gid = gids[i];
						switch(ModelCodeHelper.GetTypeFromGID(gid))
						{
							case DMSType.Analog:
								if(measurementsOfInterest.Contains(gid))
									analogs.Add(gid);
								break;

							case DMSType.Discrete:
								if(measurementsOfInterest.Contains(gid))
									discretes.Add(gid);
								break;
						}
					}
				}

				if(analogs.Count <= 0 && discretes.Count <= 0)
					return;

				List<KeyValuePair<long, float>> analogValues = null;
				List<KeyValuePair<long, int>> discreteValues = null;

				Client<ISCADAServiceContract> client = new Client<ISCADAServiceContract>("endpointSCADA");
				client.Connect();

				if(!client.Call<bool>(scada => { analogValues = scada.ReadAnalog(analogs); discreteValues = scada.ReadDiscrete(discretes); return true; }, out _))
				{
					client.Disconnect();
					return;
				}

				client.Disconnect();

				for(int i = 0; i < analogValues.Count; ++i)
				{
					KeyValuePair<long, float> tuple = analogValues[i];
					analogInputs[tuple.Key] = tuple.Value;
				}

				for(int i = 0; i < discreteValues.Count; ++i)
				{
					KeyValuePair<long, int> tuple = discreteValues[i];
					discreteInputs[tuple.Key] = tuple.Value;
				}

				rwLock.EnterWriteLock();

				try
				{
					graph = new TopologyGraph(containers, analogInputs, discreteInputs);
					lineEnergization = graph.CalculateLineEnergization();
					Publish();
				}
				finally
				{
					rwLock.ExitWriteLock();
				}
			}
			finally
			{
				rwLock.ExitUpgradeableReadLock();
			}
		}

		List<long> GetAnalogGIDsOfInterest()
		{
			return new List<long>();
		}

		List<long> GetDiscreteGIDsOfInterest()
		{
			List<long> gids = new List<long>();

			foreach(IdentifiedObject discrete in containers[DMSType.Discrete].Values)
			{
				if(IsMeasurementOfInterest(discrete))
				{
					gids.Add(discrete.GID);
				}
			}

			return gids;
		}

		bool IsMeasurementOfInterest(IdentifiedObject io)
		{
			return ModelCodeHelper.GetTypeFromGID(io.GID) == DMSType.Discrete && ((Discrete)io).MeasurementType == MeasurementType.SwitchState;
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

		void Publish()
		{
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