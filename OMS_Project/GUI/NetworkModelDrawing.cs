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

		public NetworkModelDrawing(NetworkModel networkModel)
		{
			nm = networkModel;
			Reload();
		}

		public void Reload()
		{
			Layout(nm.GetTrees());
			Redraw();
		}

		void Layout(List<Node> trees)
		{
			NodeLayout root = new NodeLayout(null, null);
			uint x = 0;

			foreach(Node tree in trees)
			{
				NodeLayout treeRoot = new NodeLayout(root, tree);
				NodeLayout node = treeRoot;
				uint y = 0;

				do
				{
					node.Y = y;

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

			this.layout = root;
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
					elements.Add(GetGE(node, model));

					foreach(NodeLayout child in node.Children)
					{
						lines.Add(GetGL(node, child));
						stack.Push(child);
					}
				}
			}

			this.elements = elements;
			this.lines = lines;
		}

		GraphicsElement GetGE(NodeLayout node, GraphicsModel model)
		{
			return new GraphicsElement() { Scale = 1, Model = model, X = node.X, Y = node.Y };
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
