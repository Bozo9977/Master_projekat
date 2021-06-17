using Common.CalculationEngine;
using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;

namespace CalculationEngine
{
	public class Node
	{
		public IdentifiedObject IO { get; set; }
		public int ChildrenOffset { get; set; }
		public int ChildrenCount { get; set; }

		public Node(IdentifiedObject io, int childrenOffset, int childrenCount)
		{
			IO = io;
			ChildrenOffset = childrenOffset;
			ChildrenCount = childrenCount;
		}
	}

	public class TopologyGraph
	{
		List<Node> subGraphs;
		List<Node> adjacency;
		Dictionary<DMSType, Dictionary<long, IdentifiedObject>> containers;
		Dictionary<DMSType, ModelCode> dmsTypeToModelCodeMap;
		IReadOnlyDictionary<long, float> analogs;
		IReadOnlyDictionary<long, int> discretes;

		public TopologyGraph(Dictionary<DMSType, Dictionary<long, IdentifiedObject>> containers, IReadOnlyDictionary<long, float> analogs, IReadOnlyDictionary<long, int> discretes)
		{
			subGraphs = new List<Node>();
			adjacency = new List<Node>();
			this.containers = containers;
			this.analogs = analogs;
			this.discretes = discretes;
			dmsTypeToModelCodeMap = ModelResourcesDesc.GetTypeToModelCodeMap();

			BuildGraph();
		}

		bool BuildGraph()
		{
			foreach(IdentifiedObject source in containers[DMSType.EnergySource].Values)
			{
				if(source != null)
				{
					subGraphs.Add(BuildSubGraph((EnergySource)source));
				}
			}

			return true;
		}

		Node BuildSubGraph(EnergySource source)
		{
			Dictionary<long, IdentifiedObject> terminals = containers[DMSType.Terminal];
			Dictionary<long, IdentifiedObject> cNodes = containers[DMSType.ConnectivityNode];

			Node sourceNode = new Node(source, 0, 0);
			Queue<Tuple<Node, Node>> queue = new Queue<Tuple<Node, Node>>();
			queue.Enqueue(new Tuple<Node, Node>(null, sourceNode));

			Dictionary<long, Node> visited = new Dictionary<long, Node>();
			visited.Add(source.GID, sourceNode);

			while(queue.Count > 0)
			{
				Tuple<Node, Node> tuple = queue.Dequeue();
				Node node = tuple.Item2;
				node.ChildrenOffset = adjacency.Count;

				if(tuple.Item1 != null)
				{
					adjacency.Add(tuple.Item1);
					++node.ChildrenCount;
				}

				DMSType type = ModelCodeHelper.GetTypeFromGID(node.IO.GID);

				if(type == DMSType.ConnectivityNode)
				{
					foreach(long tGID in ((ConnectivityNode)node.IO).Terminals)
					{
						IdentifiedObject terminal;

						if(!terminals.TryGetValue(tGID, out terminal))
							continue;

						long ceGID = ((Terminal)terminal).ConductingEquipment;

						if(ceGID == 0)
							continue;

						Node childNode;

						if(!visited.TryGetValue(ceGID, out childNode))
						{
							DMSType ceType = ModelCodeHelper.GetTypeFromGID(ceGID);
							IdentifiedObject ce;

							if(!containers[ceType].TryGetValue(ceGID, out ce))
								continue;

							childNode = new Node(ce, 0, 0);
							queue.Enqueue(new Tuple<Node, Node>(node, childNode));
							visited.Add(ceGID, childNode);
						}

						adjacency.Add(childNode);
						++node.ChildrenCount;
					}
				}
				else
				{
					foreach(long tGID in ((ConductingEquipment)node.IO).Terminals)
					{
						IdentifiedObject terminal;

						if(!terminals.TryGetValue(tGID, out terminal))
							continue;

						long cNodeGID = ((Terminal)terminal).ConnectivityNode;

						if(cNodeGID == 0)
							continue;

						Node childNode;

						if(!visited.TryGetValue(cNodeGID, out childNode))
						{
							IdentifiedObject cNode;

							if(!cNodes.TryGetValue(cNodeGID, out cNode))
								continue;

							childNode = new Node(cNode, 0, 0);
							queue.Enqueue(new Tuple<Node, Node>(node, childNode));
							visited.Add(cNodeGID, childNode);
						}

						adjacency.Add(childNode);
						++node.ChildrenCount;
					}
				}
			}

			return sourceNode;
		}

		public List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> CalculateLineEnergization()
		{
			List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> sourcesEnergization = new List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>>(subGraphs.Count);

			for(int i = 0; i < subGraphs.Count; ++i)
			{
				Stack<Tuple<Node, EEnergization>> stack = new Stack<Tuple<Node, EEnergization>>();
				stack.Push(new Tuple<Node, EEnergization>(subGraphs[i], EEnergization.Energized));
				HashSet<Tuple<long, long>> visitedEnergized = new HashSet<Tuple<long, long>>();
				HashSet<Tuple<long, long>> visitedUnknown = new HashSet<Tuple<long, long>>();

				while(stack.Count > 0)
				{
					Tuple<Node, EEnergization> node = stack.Pop();
					EEnergization energization = node.Item2;
					long gid = node.Item1.IO.GID;

					if(energization != EEnergization.NotEnergized && ModelCodeHelper.ModelCodeClassIsSubClassOf(dmsTypeToModelCodeMap[ModelCodeHelper.GetTypeFromGID(gid)], ModelCode.SWITCH))
					{
						Switch sw = (Switch)node.Item1.IO;
						Discrete d = null;

						for(int k = 0; k < sw.Measurements.Count; ++k)
						{
							long measGID = sw.Measurements[k];
							IdentifiedObject meas;
							Discrete discrete;

							if(ModelCodeHelper.GetTypeFromGID(measGID) == DMSType.Discrete && containers[DMSType.Discrete].TryGetValue(measGID, out meas))
							{
								discrete = (Discrete)meas;

								if(discrete.MeasurementType == MeasurementType.SwitchState)
								{
									d = discrete;
									break;
								}
							}
						}

						int switchState;

						if(d == null || !discretes.TryGetValue(d.GID, out switchState))
						{
							energization = EEnergization.Unknown;
						}
						else if(switchState != 0)
						{
							energization = EEnergization.NotEnergized;
						}
					}

					if(energization == EEnergization.NotEnergized)
						continue;

					if(energization == EEnergization.Energized)
					{
						for(int j = node.Item1.ChildrenOffset; j < node.Item1.ChildrenOffset + node.Item1.ChildrenCount; ++j)
						{
							Node childNode = adjacency[j];
							long childGid = childNode.IO.GID;
							Tuple<long, long> line = gid <= childGid ? new Tuple<long, long>(gid, childGid) : new Tuple<long, long>(childGid, gid);

							if(visitedEnergized.Contains(line))
								continue;

							stack.Push(new Tuple<Node, EEnergization>(childNode, EEnergization.Energized));

							visitedEnergized.Add(line);
							visitedUnknown.Remove(line);
						}
					}
					else
					{
						for(int j = node.Item1.ChildrenOffset; j < node.Item1.ChildrenOffset + node.Item1.ChildrenCount; ++j)
						{
							Node childNode = adjacency[j];
							long childGid = childNode.IO.GID;
							Tuple<long, long> line = gid <= childGid ? new Tuple<long, long>(gid, childGid) : new Tuple<long, long>(childGid, gid);

							if(visitedEnergized.Contains(line) || visitedUnknown.Contains(line))
								continue;

							stack.Push(new Tuple<Node, EEnergization>(childNode, EEnergization.Unknown));

							visitedUnknown.Add(line);
						}
					}
				}

				sourcesEnergization.Add(new Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>(subGraphs[i].IO.GID, new List<Tuple<long, long>>(visitedEnergized), new List<Tuple<long, long>>(visitedUnknown)));
			}

			return sourcesEnergization;
		}
	}
}
