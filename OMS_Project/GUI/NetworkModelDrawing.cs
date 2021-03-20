using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Shapes;

namespace GUI
{
	class NodeLayout
	{
		public uint X { get; set; }
		public uint Y { get; set; }
		public Node Node { get; private set; }
		public NodeLayout Parent { get; private set; }
		public List<NodeLayout> Children { get; private set; }

		public NodeLayout(NodeLayout parent, Node node)
		{
			Node = node;
			Parent = parent;
			Children = new List<NodeLayout>();
		}
	}

	class NetworkModelDrawing
	{
		NetworkModel nm;
		List<GraphicsElement> elements;
		List<GraphicsLine> lines;
		NodeLayout layout;
		List<Tuple<NodeLayout, NodeLayout>> reclosers;
		Dictionary<DMSType, object> typeToGE;

		public NetworkModelDrawing(NetworkModel networkModel)
		{
			typeToGE = new Dictionary<DMSType, object>();

			typeToGE[DMSType.ACLineSegment] = new GraphicsModels.ACLineSegment();
			typeToGE[DMSType.Breaker] = new GraphicsModels.Breaker();
			typeToGE[DMSType.ConnectivityNode] = new GraphicsModels.ConnectivityNode();
			typeToGE[DMSType.Disconnector] = new GraphicsModels.Disconnector();
			typeToGE[DMSType.DistributionGenerator] = new GraphicsModels.DistributionGenerator();
			typeToGE[DMSType.EnergyConsumer] = new Dictionary<ConsumerClass, GraphicsModel> { { ConsumerClass.Administrative, new GraphicsModels.AdministrativeConsumer() }, { ConsumerClass.Residential, new GraphicsModels.ResidentialConsumer() }, { ConsumerClass.Industrial, new GraphicsModels.IndustrialConsumer() } };
			typeToGE[DMSType.EnergySource] = new GraphicsModels.EnergySource();
			typeToGE[DMSType.Recloser] = new GraphicsModels.Recloser();
			typeToGE[DMSType.TransformerWinding] = new GraphicsModels.TransformerWinding();

			nm = networkModel;
			Reload();
		}

		public void Reload()
		{
			Layout(nm.GetTreesAndReclosers());
			Redraw();
		}

		void Layout(Tuple<List<Node>, List<RecloserNode>> model)
		{
			NodeLayout root = new NodeLayout(null, null);
			uint x = 0;

			Dictionary<Node, NodeLayout> recloserConNodes = new Dictionary<Node, NodeLayout>(model.Item2.Count * 2);

			foreach(RecloserNode rn in model.Item2)
			{
				recloserConNodes[rn.node1] = null;
				recloserConNodes[rn.node2] = null;
			}

			foreach(Node tree in model.Item1)
			{
				NodeLayout treeRoot = new NodeLayout(root, tree);
				NodeLayout node = treeRoot;
				uint y = 0;

				do
				{
					node.Y = y;

					if(recloserConNodes.ContainsKey(node.Node))
						recloserConNodes[node.Node] = node;

					if(node.Children.Count < node.Node.children.Count)
					{
						node = new NodeLayout(node, node.Node.children[node.Children.Count]);
						++y;
						continue;
					}

					if(node.Children.Count > 0)
					{
						foreach(NodeLayout child in node.Children)
							node.X += child.X;

						node.X /= (uint)node.Children.Count;
					}
					else
					{
						node.X = x++;
					}

					node.Parent.Children.Add(node);
					node = node.Parent;
					--y;

					
				}
				while(node != root);
			}

			List<Tuple<NodeLayout, NodeLayout>> reclosers = new List<Tuple<NodeLayout, NodeLayout>>(model.Item2.Count);

			foreach(RecloserNode rn in model.Item2)
			{
				reclosers.Add(new Tuple<NodeLayout, NodeLayout>(recloserConNodes[rn.node1], recloserConNodes[rn.node2]));
			}

			this.layout = root;
			this.reclosers = reclosers;
		}

		void Redraw()
		{
			GraphicsModels.ConnectivityNode model = new GraphicsModels.ConnectivityNode();
			List<GraphicsElement> elements = new List<GraphicsElement>();
			List<GraphicsLine> lines = new List<GraphicsLine>();

			foreach(NodeLayout tree in layout.Children)
			{
				Stack<NodeLayout> stack = new Stack<NodeLayout>();
				stack.Push(tree);

				while(stack.Count > 0)
				{
					NodeLayout node = stack.Pop();
					elements.Add(GetGE(node));

					foreach(NodeLayout child in node.Children)
					{
						lines.Add(GetGL(node, child));
						stack.Push(child);
					}
				}
			}

			foreach(Tuple<NodeLayout, NodeLayout> r in reclosers)
			{
				lines.Add(GetGL(r.Item1, r.Item2));
			}

			this.elements = elements;
			this.lines = lines;
		}

		GraphicsElement GetGE(NodeLayout node)
		{
			DMSType type = ModelCodeHelper.GetTypeFromGID(node.Node.io.GID);
			object model;

			if(!typeToGE.TryGetValue(type, out model))
				return null;

			if(type == DMSType.EnergyConsumer)
			{
				model = ((Dictionary<ConsumerClass, GraphicsModel>)model)[((EnergyConsumer)(node.Node.io)).ConsumerClass];
			}

			return new GraphicsElement() { Scale = 0.5, Model = (GraphicsModel)model, X = node.X, Y = node.Y };
		}

		GraphicsLine GetGL(NodeLayout node1, NodeLayout node2)
		{
			return new GraphicsLine() { X1 = node1.X, Y1 = node1.Y, X2 = node2.X, Y2 = node2.Y, Thickness = 2 };
		}

		public Tuple<List<GraphicsElement>, List<GraphicsLine>> Draw()
		{
			return new Tuple<List<GraphicsElement>, List<GraphicsLine>>(elements, lines);
		}
	}
}
