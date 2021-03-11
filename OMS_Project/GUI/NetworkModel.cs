using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	class NetworkModel
	{
		class Node
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

		List<Node> trees;

		public NetworkModel(NetworkModelDownload download)
		{
			Dictionary<long, IdentifiedObject> terminalContainer = download.Containers[DMSType.Terminal];
			Dictionary<long, IdentifiedObject> recloserContainer = download.Containers[DMSType.Recloser];
			Dictionary<long, IdentifiedObject> conNodeContainer = download.Containers[DMSType.ConnectivityNode];

			trees = new List<Node>();

			foreach(KeyValuePair<long, IdentifiedObject> source in download.Containers[DMSType.EnergySource])
			{
				List<Node> tree = new List<Node>();

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
									continue;

								Node childNode = new Node(node, condEq);
								node.children.Insert(0, childNode);
								stack.Push(childNode);
							}
						}
						break;

						default:
						{
							ConductingEquipment condEq = io as ConductingEquipment;

							if(io == null)
								break;

							foreach(long terminalGID in condEq.Terminals)
							{
								long conNodeGID = ((Terminal)terminalContainer[terminalGID]).ConnectivityNode;

								if(node.parent != null && conNodeGID == node.parent.io.GID)
									continue;

								Node childNode = new Node(node, conNodeContainer[conNodeGID]);
								node.children.Insert(0, childNode);
								stack.Push(childNode);
							}
						}
						break;
					}
				}

				trees.Add(root);
			}
		}
	}
}
