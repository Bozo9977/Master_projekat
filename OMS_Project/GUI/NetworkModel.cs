using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GUI
{
	public class Node
	{
		public Node parent;
		public IdentifiedObject io;
		public List<Node> children;

		public Node(Node parent, IdentifiedObject io)
		{
			this.parent = parent;
			this.io = io;
			children = new List<Node>();
		}
	}

	public class RecloserNode
	{
		public Node node1, node2;
		public IdentifiedObject io;

		public RecloserNode(IdentifiedObject io)
		{
			this.io = io;
		}
	}

	public class NetworkModel
	{
		Dictionary<DMSType, Dictionary<long, IdentifiedObject>> containers;
		List<Node> trees;
		Dictionary<long, RecloserNode> reclosers;

		public NetworkModel(NetworkModelDownload download)
		{
			Dictionary<long, IdentifiedObject> terminalContainer = download.Containers[DMSType.Terminal];
			Dictionary<long, IdentifiedObject> conNodeContainer = download.Containers[DMSType.ConnectivityNode];

			List<Node> trees = new List<Node>();
			Dictionary<long, RecloserNode> reclosers = new Dictionary<long, RecloserNode>();

			foreach(KeyValuePair<long, IdentifiedObject> source in download.Containers[DMSType.EnergySource])
			{
				if(source.Value == null)
				{
					continue;
				}	

				Node root = new Node(null, source.Value);
				Stack<Node> stack = new Stack<Node>();
				stack.Push(root);

				while(stack.Count > 0)
				{
					Node node = stack.Pop();
					IdentifiedObject io = node.io;
					DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);

					switch(type)
					{
						case DMSType.ConnectivityNode:
						{
							foreach(long terminalGID in ((ConnectivityNode)io).Terminals)
							{
								long condEqGID = ((Terminal)terminalContainer[terminalGID]).ConductingEquipment;

								if(condEqGID == node.parent.io.GID)
									continue;

								DMSType condEqType = ModelCodeHelper.GetTypeFromGID(condEqGID);
								ConductingEquipment condEq = (ConductingEquipment)download.Containers[condEqType][condEqGID];

								if(condEq is Recloser)
								{
									RecloserNode rn;
									if(reclosers.TryGetValue(condEqGID, out rn))
									{
										if(rn.node2 == null)
											rn.node2 = node;
									}
									else
									{
										reclosers.Add(condEqGID, new RecloserNode(condEq) { node1 = node });
									}

									continue;
								}

								Node childNode = new Node(node, condEq);
								node.children.Add(childNode);
								stack.Push(childNode);
							}
						}
						break;

						case DMSType.TransformerWinding:
						{
							TransformerWinding tw = io as TransformerWinding;

							if(tw == null)
								break;

							PowerTransformer pt = GetInternal(download.Containers, tw.PowerTransformer) as PowerTransformer;

							if(pt == null)
								break;

							foreach(long twGID in pt.TransformerWindings)
							{
								if(twGID == tw.GID || (node.parent != null && twGID == node.parent.io.GID))
									continue;

								TransformerWinding twChild = GetInternal(download.Containers, twGID) as TransformerWinding;

								if(twChild == null)
									continue;

								Node childNode = new Node(node, twChild);
								node.children.Add(childNode);
								stack.Push(childNode);
							}

							foreach(long terminalGID in tw.Terminals)
							{
								long conNodeGID = ((Terminal)terminalContainer[terminalGID]).ConnectivityNode;

								if(node.parent != null && conNodeGID == node.parent.io.GID)
									continue;

								Node childNode = new Node(node, conNodeContainer[conNodeGID]);
								node.children.Add(childNode);
								stack.Push(childNode);
							}
						}
						break;

						default:
						{
							ConductingEquipment condEq = io as ConductingEquipment;

							if(condEq == null)
								break;

							foreach(long terminalGID in condEq.Terminals)
							{
								long conNodeGID = ((Terminal)terminalContainer[terminalGID]).ConnectivityNode;

								if(node.parent != null && conNodeGID == node.parent.io.GID)
									continue;

								Node childNode = new Node(node, conNodeContainer[conNodeGID]);
								node.children.Add(childNode);
								stack.Push(childNode);
							}
						}
						break;
					}
				}

				trees.Add(root);
			}

			this.trees = trees;
			this.reclosers = reclosers;
			this.containers = download.Containers;
		}

		public IEnumerable<long> GetGIDsByType(DMSType type)
		{
			Dictionary<long, IdentifiedObject> container;

			if(!containers.TryGetValue(type, out container))
				return new long[0];

			return container.Keys;
		}

		public Tuple<List<Node>, List<RecloserNode>> GetTreesAndReclosers()
		{
			return new Tuple<List<Node>, List<RecloserNode>>(trees, reclosers.Values.ToList());
		}

		public IdentifiedObject Get(long gid)
		{
			return GetInternal(containers, gid);
		}

		IdentifiedObject GetInternal(Dictionary<DMSType, Dictionary<long, IdentifiedObject>> containers, long gid)
		{
			IdentifiedObject io;
			Dictionary<long, IdentifiedObject> container;
			return containers.TryGetValue(ModelCodeHelper.GetTypeFromGID(gid), out container) && container.TryGetValue(gid, out io) ? io : null;
		}

		public long GetSwitchSignal(long gid)
		{
			Switch sw = Get(gid) as Switch;

			if(sw == null)
				return 0;

			for(int j = 0; j < sw.Measurements.Count; ++j)
			{
				long measGID = sw.Measurements[j];

				if(ModelCodeHelper.GetTypeFromGID(measGID) != DMSType.Discrete)
					continue;

				Discrete d = Get(measGID) as Discrete;

				if(d == null || d.MeasurementType != MeasurementType.SwitchState)
					continue;

				return measGID;
			}

			return 0;
		}
	}
}
