using Common.CalculationEngine;
using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;

namespace CalculationEngine
{
	public class Node
	{
		public Node Parent { get; set; }
		public IdentifiedObject IO { get; set; }
		public int ChildrenOffset { get; set; }
		public int ChildrenCount { get; set; }

		public Node(Node parent, IdentifiedObject io, int childrenOffset, int childrenCount)
		{
			Parent = parent;
			IO = io;
			ChildrenOffset = childrenOffset;
			ChildrenCount = childrenCount;
		}
	}

	public class RecloserNode
	{
		public Node Node1 { get; set; }
		public Node Node2 { get; set; }
		public Recloser IO { get; set; }

		public RecloserNode(Recloser io, Node node1, Node node2)
		{
			IO = io;
			Node1 = node1;
			Node2 = node2;
		}
	}

	public class TopologyGraph
	{
		List<Node> trees;
		List<Node> nodeChildren;
		Dictionary<long, RecloserNode> reclosers;
		Dictionary<DMSType, Dictionary<long, IdentifiedObject>> containers;
		Dictionary<DMSType, ModelCode> dmsTypeToModelCodeMap;

		public TopologyGraph(Dictionary<DMSType, Dictionary<long, IdentifiedObject>> containers)
		{
			trees = new List<Node>();
			nodeChildren = new List<Node>();
			this.containers = containers;
			dmsTypeToModelCodeMap = ModelResourcesDesc.GetTypeToModelCodeMap();

			BuildGraph();
		}

		bool BuildGraph()
		{
			foreach(IdentifiedObject source in containers[DMSType.EnergySource].Values)
			{
				if(source == null)
					continue;

				trees.Add(BuildTree((EnergySource)source));
			}

			return true;
		}

		Node BuildTree(IdentifiedObject source)
		{
			Dictionary<long, IdentifiedObject> terminals = containers[DMSType.Terminal];
			Dictionary<long, IdentifiedObject> cNodes = containers[DMSType.ConnectivityNode];

			Node sourceNode = new Node(null, source, 0, 0);
			Queue<Node> queue = new Queue<Node>();
			queue.Enqueue(sourceNode);

			HashSet<long> visited = new HashSet<long>();
			visited.Add(source.GID);

			while(queue.Count > 0)
			{
				Node node = queue.Dequeue();
				node.ChildrenOffset = nodeChildren.Count;
				DMSType type = ModelCodeHelper.GetTypeFromGID(node.IO.GID);

				if(type == DMSType.ConnectivityNode)
				{
					foreach(long tGID in ((ConnectivityNode)node.IO).Terminals)
					{
						IdentifiedObject terminal;

						if(!terminals.TryGetValue(tGID, out terminal))
							continue;

						long ceGID = ((Terminal)terminal).ConductingEquipment;

						if(ceGID == 0 || visited.Contains(ceGID))
							continue;

						DMSType ceType = ModelCodeHelper.GetTypeFromGID(ceGID);
						IdentifiedObject ce;

						if(!containers[ceType].TryGetValue(ceGID, out ce))
							continue;

						Recloser recloser = ce as Recloser;

						if(recloser != null)
						{
							RecloserNode rn;
							if(reclosers.TryGetValue(ceGID, out rn))
							{
								if(rn.Node2 == null)
									rn.Node2 = node;
							}
							else
							{
								reclosers.Add(ceGID, new RecloserNode(recloser, node, null));
							}

							continue;
						}

						Node childNode = new Node(node, ce, 0, 0);
						nodeChildren.Add(childNode);
						++node.ChildrenCount;
						queue.Enqueue(childNode);
						visited.Add(ceGID);
					}
				}
				else
				{
					ConductingEquipment ce = (ConductingEquipment)node.IO;

					foreach(long tGID in ce.Terminals)
					{
						IdentifiedObject terminal;

						if(!terminals.TryGetValue(tGID, out terminal))
							continue;

						long cNodeGID = ((Terminal)terminal).ConnectivityNode;
						IdentifiedObject cNode;

						if(cNodeGID == 0 || visited.Contains(cNodeGID) || !cNodes.TryGetValue(cNodeGID, out cNode))
							continue;

						Node childNode = new Node(node, cNode, 0, 0);
						nodeChildren.Add(childNode);
						++node.ChildrenCount;
						queue.Enqueue(childNode);
						visited.Add(cNodeGID);
					}
				}
			}

			return sourceNode;
		}

		public List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> CalculateLineEnergization()
		{
			List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> sourcesEnergization = new List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>>(trees.Count);

			for(int i = 0; i < trees.Count; ++i)
			{
				Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>> sourceEnergization = new Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>(trees[i].IO.GID, new List<Tuple<long, long>>(), new List<Tuple<long, long>>());

				Stack<Tuple<Node, EEnergization>> stack = new Stack<Tuple<Node, EEnergization>>();
				stack.Push(new Tuple<Node, EEnergization>(trees[i], EEnergization.Energized));

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

						if(d == null || !Measurements.Instance.TryGetDiscrete(d.GID, out switchState))
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

					List<Tuple<long, long>> lines = energization == EEnergization.Energized ? sourceEnergization.Item2 : sourceEnergization.Item3;

					for(int j = node.Item1.ChildrenOffset; j < node.Item1.ChildrenOffset + node.Item1.ChildrenCount; ++j)
					{
						Node childNode = nodeChildren[j];
						stack.Push(new Tuple<Node, EEnergization>(childNode, energization));
						long childGid = childNode.IO.GID;
						lines.Add(gid <= childGid ? new Tuple<long, long>(gid, childGid) : new Tuple<long, long>(childGid, gid));
					}
				}

				sourcesEnergization.Add(sourceEnergization);
			}

			return sourcesEnergization;
		}
	}
}
