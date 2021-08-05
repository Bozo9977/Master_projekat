using Common;
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
		List<KeyValuePair<long, LoadFlowResult>> loadFlowResults;
		ConcurrentDictionary<long, float> analogInputs;
		ConcurrentDictionary<long, int> discreteInputs;
		HashSet<long> measurementsOfInterest;
		ConcurrentDictionary<long, bool> markedSwitchStates;
		Dictionary<DMSType, ModelCode> typeToModelCode;
		List<DailyLoadProfile> loadProfiles;

		public TopologyModel(List<DailyLoadProfile> loadProfiles)
		{
			DMSType[] types = ModelResourcesDesc.TypeIdsInInsertOrder;
			containers = new Dictionary<DMSType, Dictionary<long, IdentifiedObject>>(types.Length);

			foreach(DMSType t in types)
				containers.Add(t, new Dictionary<long, IdentifiedObject>());

			rwLock = new ReaderWriterLockSlim();
			analogInputs = new ConcurrentDictionary<long, float>();
			discreteInputs = new ConcurrentDictionary<long, int>();
			measurementsOfInterest = new HashSet<long>();
			markedSwitchStates = new ConcurrentDictionary<long, bool>();
			typeToModelCode = ModelResourcesDesc.GetTypeToModelCodeMap();
			this.loadProfiles = loadProfiles;
			graph = new TopologyGraph(containers, analogInputs, discreteInputs, markedSwitchStates, loadProfiles);
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
				markedSwitchStates = new ConcurrentDictionary<long, bool>(tm.markedSwitchStates);
				typeToModelCode = ModelResourcesDesc.GetTypeToModelCodeMap();
				loadProfiles = new List<DailyLoadProfile>(tm.loadProfiles);
				graph = tm.graph;
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

					measurementsOfInterest.Add(io.GID);
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
					markedSwitchStates.TryRemove(gid, out _);
				}

				foreach(long switchGID in markedSwitchStates.Keys)
				{
					if(!IsSwitchWithoutSCADA(switchGID))
						markedSwitchStates.TryRemove(switchGID, out _);
				}

				graph = new TopologyGraph(containers, analogInputs, discreteInputs, markedSwitchStates, loadProfiles);

				return true;
			}
			finally
			{
				rwLock.ExitWriteLock();
			}
		}

		bool IsSwitchWithoutSCADA(long switchGID)
		{
			if(!ModelCodeHelper.ModelCodeClassIsSubClassOf(typeToModelCode[ModelCodeHelper.GetTypeFromGID(switchGID)], ModelCode.SWITCH))
				return false;

			Switch s = Get(switchGID) as Switch;

			if(s == null)
				return false;

			for(int i = 0; i < s.Measurements.Count; ++i)
			{
				Measurement m = Get(s.Measurements[i]) as Measurement;

				if(m == null)
					continue;

				if(m.MeasurementType == MeasurementType.SwitchState)
					return false;
			}

			return true;
		}

		IdentifiedObject Get(long gid)
		{
			IdentifiedObject io;
			Dictionary<long, IdentifiedObject> container;
			return containers.TryGetValue(ModelCodeHelper.GetTypeFromGID(gid), out container) && container.TryGetValue(gid, out io) ? io : null;
		}

		public bool MarkSwitchState(long gid, bool open)
		{
			rwLock.EnterWriteLock();

			try
			{
				if(!IsSwitchWithoutSCADA(gid))
				{
					return false;
				}

				markedSwitchStates[gid] = open;

				lineEnergization = graph.CalculateLineEnergization();
				loadFlowResults = graph.CalculateLoadFlow();
			}
			finally
			{
				rwLock.ExitWriteLock();
			}

			Publish(new MarkedSwitchesChanged());
			Publish(new TopologyChanged());
			Publish(new LoadFlowChanged());

			return true;
		}

		public bool UnmarkSwitchState(long gid)
		{
			rwLock.EnterWriteLock();

			try
			{
				if(!IsSwitchWithoutSCADA(gid) || !markedSwitchStates.TryRemove(gid, out _))
					return false;

				lineEnergization = graph.CalculateLineEnergization();
				loadFlowResults = graph.CalculateLoadFlow();
			}
			finally
			{
				rwLock.ExitWriteLock();
			}

			Publish(new MarkedSwitchesChanged());
			Publish(new TopologyChanged());
			Publish(new LoadFlowChanged());

			return true;
		}

		public List<KeyValuePair<long, bool>> GetMarkedSwitches()
		{
			rwLock.EnterReadLock();

			try
			{
				return new List<KeyValuePair<long, bool>>(markedSwitchStates);
			}
			finally
			{
				rwLock.ExitReadLock();
			}
		}

		public void DownloadMeasurements(List<long> gids)
		{
			bool publish = false;
			rwLock.EnterUpgradeableReadLock();

			try
			{
				List<long> analogs;
				List<long> discretes;
				bool update = false;

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
							{
								analogs.Add(gid);

								IdentifiedObject io;
								containers[DMSType.Analog].TryGetValue(gid, out io);

								if(IsMeasurementOfInterest(io))
									update = true;
							}
							break;

							case DMSType.Discrete:
							{
								discretes.Add(gid);

								IdentifiedObject io;
								containers[DMSType.Discrete].TryGetValue(gid, out io);

								if(IsMeasurementOfInterest(io))
									update = true;
							}
							break;
						}
					}
				}

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

				if(lineEnergization != null && !update)
					return;

				rwLock.EnterWriteLock();

				try
				{
					lineEnergization = graph.CalculateLineEnergization();
					loadFlowResults = graph.CalculateLoadFlow();
					publish = true;
				}
				catch(Exception e)
				{

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

			if(publish)
			{
				Publish(new TopologyChanged());
				Publish(new LoadFlowChanged());
			}
		}

		List<long> GetAnalogGIDsOfInterest()
		{
			return new List<long>(containers[DMSType.Analog].Keys);
		}

		List<long> GetDiscreteGIDsOfInterest()
		{
			return new List<long>(containers[DMSType.Discrete].Keys);
		}

		bool IsMeasurementOfInterest(IdentifiedObject io)
		{
			Discrete d;
			return (d = io as Discrete) != null && d.MeasurementType == MeasurementType.SwitchState;
		}

		public List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> GetLineEnergization()
		{
			return lineEnergization;
		}

		public List<KeyValuePair<long, LoadFlowResult>> GetLoadFlow()
		{
			return loadFlowResults;
		}

		void Publish(PubSubMessage msg)
		{
			Client<IPublishing> pubClient = new Client<IPublishing>("publishingEndpoint");
			pubClient.Connect();

			pubClient.Call<bool>(pub =>
			{
				pub.Publish(msg);
				return true;
			}, out _);

			pubClient.Disconnect();
		}
	}
}