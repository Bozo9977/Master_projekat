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
		//public uint SubtreeDepth { get; set; }
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

	class RecloserLayout
	{
		public NodeLayout Node1 { get; private set; }
		public NodeLayout Node2 { get; private set; }

		public RecloserLayout(NodeLayout node1, NodeLayout node2)
		{
			Node1 = node1;
			Node2 = node2;
		}
	}

	class RecloserState : IComparable<RecloserState>
	{
		public RecloserLayout Recloser { get; private set; }
		public NodeLayout Parent { get; private set; }
		public Stack<NodeLayout> First { get; private set; }
		public Stack<NodeLayout> Second { get; private set; }
		public uint Depth { get; private set; }

		public RecloserState(RecloserLayout recloser)
		{
			Recloser = recloser;
			First = new Stack<NodeLayout>();
			Second = new Stack<NodeLayout>();
			NodeLayout n1 = recloser.Node1;
			NodeLayout n2 = recloser.Node2;

			if(n1.Y < n2.Y)
			{
				NodeLayout t = n1;
				n1 = n2;
				n2 = t;
			}

			while(n1.Y > n2.Y)
			{
				First.Push(n1);
				n1 = n1.Parent;
			}

			while(n1 != n2)
			{
				First.Push(n1);
				Second.Push(n2);
				n1 = n1.Parent;
				n2 = n2.Parent;
			}

			Parent = n1;
			Depth = recloser.Node1.Y - n1.Y;
		}

		public int CompareTo(RecloserState other)
		{
			return ((Depth > other.Depth) ? 1 : 0) - ((Depth < other.Depth) ? 1 : 0);
		}

		public void Swap()
		{
			Stack<NodeLayout> temp = First;
			First = Second;
			Second = temp;
		}
	}

	class NetworkModelDrawing
	{
		NetworkModel nm;
		List<GraphicsElement> elements;
		List<GraphicsLine> lines;
		NodeLayout root;
		List<RecloserLayout> reclosers;
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
			Dictionary<Node, NodeLayout> recloserConNodes = new Dictionary<Node, NodeLayout>(model.Item2.Count * 2);

			foreach(RecloserNode rn in model.Item2)
			{
				recloserConNodes[rn.node1] = null;
				recloserConNodes[rn.node2] = null;
			}

			NodeLayout root = new NodeLayout(null, null);

			foreach(Node tree in model.Item1)
			{
				NodeLayout treeRoot = new NodeLayout(root, tree);
				NodeLayout node = treeRoot;
				uint y = 0;

				do
				{
					node.Y = y;
					//node.SubtreeDepth = y;

					if(recloserConNodes.ContainsKey(node.Node))
						recloserConNodes[node.Node] = node;

					if(node.Children.Count < node.Node.children.Count)
					{
						node = new NodeLayout(node, node.Node.children[node.Children.Count]);
						++y;
						continue;
					}

					/*foreach(NodeLayout child in node.Children)
					{
						if(child.SubtreeDepth > node.SubtreeDepth)
							node.SubtreeDepth = child.SubtreeDepth;
					}*/

					node.Parent.Children.Add(node);
					node = node.Parent;
					--y;
				}
				while(node != root);

				/*if(treeRoot.SubtreeDepth > root.SubtreeDepth)
					root.SubtreeDepth = treeRoot.SubtreeDepth;*/
			}

			List<RecloserLayout> reclosers = new List<RecloserLayout>(model.Item2.Count);
			List<RecloserState> recloserStates = new List<RecloserState>(model.Item2.Count);

			foreach(RecloserNode rn in model.Item2)
			{
				RecloserLayout r = new RecloserLayout(recloserConNodes[rn.node1], recloserConNodes[rn.node2]);
				reclosers.Add(r);
				recloserStates.Add(new RecloserState(r));
			}

			recloserStates.Sort();

			{
				NodeLayout node = root;
				Stack<int> stack = new Stack<int>();
				stack.Push(0);
				uint x = 0;

				do
				{
					int i = stack.Pop();

					if(i == 0)
					{
						ReorderChildren(node, recloserStates);
					}

					if(i < node.Children.Count)
					{
						node = node.Children[i];
						stack.Push(i + 1);
						stack.Push(0);
						continue;
					}

					if(node.Children.Count > 0)
					{
						foreach(NodeLayout child in node.Children)
						{
							node.X += child.X;
						}

						node.X /= (uint)node.Children.Count;
					}
					else
					{
						node.X = x++;
					}

					node = node.Parent;
				}
				while(node != null);
			}

			this.root = root;
			this.reclosers = reclosers;
		}

		uint Min(uint x, uint y)
		{
			return x < y ? x : y;
		}

		uint Max(uint x, uint y)
		{
			return x > y ? x : y;
		}

		void ReorderChildren(NodeLayout node, List<RecloserState> recloserStates)
		{
			List<NodeLayout> children = new List<NodeLayout>(node.Children.Count + 2) { null, null };
			List<uint> depths = new List<uint>() { uint.MaxValue };

			foreach(RecloserState r in recloserStates)
			{
				NodeLayout n1 = r.First.Count > 0 ? r.First.Peek() : null;
				NodeLayout n2 = r.Second.Count > 0 ? r.Second.Peek() : null;
				int indexNode1 = n1 == null ? -1 : node.Children.IndexOf(n1);
				int indexNode2 = n2 == null ? -1 : node.Children.IndexOf(n2);

				if(indexNode1 >= 0)
				{
					r.First.Pop();
				}

				if(indexNode2 >= 0)
				{
					r.Second.Pop();
				}

				if(indexNode1 >= 0)
				{
					if(indexNode2 >= 0)
					{
						//handle parent, reorder First and Second
						int i1 = children.IndexOf(n1);
						int i2 = children.IndexOf(n2);

						if(i1 >= 0)
						{
							if(i2 <= 0)
							{
								uint d;

								if(depths[i1 - 1] > depths[i1])
								{
									i2 = i1;
									d = depths[i1 - 1];
								}
								else
								{
									i2 = i1 + 1;
									d = depths[i1];
								}

								children.Insert(i2, n2);
								depths.Insert(i1, d);
							}
						}
						else
						{
							if(i2 >= 0)
							{
								uint d;

								if(depths[i2 - 1] > depths[i2])
								{
									i1 = i2;
									d = depths[i2 - 1];
								}
								else
								{
									i1 = i2 + 1;
									d = depths[i2];
								}

								children.Insert(i1, n1);
								depths.Insert(i2, d);
							}
							else
							{
								int insertIndex;
								int depthIndex;
								uint d1 = depths[0];
								uint d2 = depths[depths.Count - 1];
								uint d;

								if(d1 >= d2)
								{
									insertIndex = 1;
									depthIndex = 1;
									d = d1;
								}
								else
								{
									insertIndex = children.Count - 1;
									depthIndex = children.Count;
									d = d2;
								}

								children.Insert(insertIndex, n1);
								children.Insert(insertIndex + 1, n2);
								depths.Insert(insertIndex, d);
								depths.Insert(insertIndex + 1, d);

								i1 = insertIndex;
								i2 = insertIndex + 1;
							}
						}

						if(i1 > i2)
						{
							int t = i1;
							i1 = i2;
							i2 = t;

							r.Swap();
						}

						for(int i = i1; i < i2; ++i)
						{
							depths[i] = Min(depths[i], r.Depth);
						}
					}
					else
					{
						//first goes right
						int index = children.IndexOf(n1);

						if(index < 0)
						{
							uint maxd = 0;
							int maxi = depths.Count - 1;

							for(int i = depths.Count - 1; i >= 0; --i)
							{
								if(depths[i] > maxd)
								{
									maxd = depths[i];
									maxi = i;
								}
							}

							children.Insert(maxi + 1, n1);
							depths.Insert(maxi + 1, Min(r.Depth, maxd));

							for(int i = maxi + 2; i < depths.Count; ++i)
							{
								if(depths[i] > r.Depth)
									depths[i] = r.Depth;
							}
						}
						else
						{
							for(int i = index; i < depths.Count; ++i)
							{
								if(depths[i] > r.Depth)
									depths[i] = r.Depth;
							}
						}
					}
				}
				else
				{
					//second goes left
					if(indexNode2 >= 0)
					{
						int index = children.IndexOf(n2);

						if(index < 0)
						{
							uint maxd = 0;
							int maxi = 0;

							for(int i = 0; i < depths.Count; ++i)
							{
								if(depths[i] > maxd)
								{
									maxd = depths[i];
									maxi = i;
								}
							}

							children.Insert(maxi + 1, n2);
							depths.Insert(maxi, Min(r.Depth, maxd));

							for(int i = maxi - 1; i >= 0; --i)
							{
								if(depths[i] > r.Depth)
									depths[i] = r.Depth;
							}
						}
						else
						{
							for(int i = index - 1; i >= 0; --i)
							{
								if(depths[i] > r.Depth)
									depths[i] = r.Depth;
							}
						}
					}
					else
					{
						//nothing
					}
				}
			}

			children.RemoveAt(0);
			children.RemoveAt(children.Count - 1);

			uint maxDepth = 0;
			int maxIndex = 0;

			for(int i = 0; i < depths.Count; ++i)
			{
				if(depths[i] > maxDepth)
				{
					maxDepth = depths[i];
					maxIndex = i;
				}
			}

			foreach(NodeLayout child in node.Children)
			{
				if(!children.Contains(child))
					children.Insert(maxIndex, child);
			}

			node.Children.Clear();
			node.Children.AddRange(children);
		}

		void Redraw()
		{
			List<GraphicsElement> elements = new List<GraphicsElement>();
			List<GraphicsLine> lines = new List<GraphicsLine>();

			foreach(NodeLayout tree in root.Children)
			{
				Stack<NodeLayout> stack = new Stack<NodeLayout>();
				stack.Push(tree);

				while(stack.Count > 0)
				{
					NodeLayout node = stack.Pop();
					elements.Add(GetElement(node));

					foreach(NodeLayout child in node.Children)
					{
						lines.Add(GetLine(node, child));
						stack.Push(child);
					}
				}
			}

			foreach(RecloserLayout r in reclosers)
			{
				elements.Add(new GraphicsElement() { Scale = 0.5, Model = (GraphicsModel)typeToGE[DMSType.Recloser], X = r.Node1.X * 0.5 + r.Node2.X * 0.5, Y = r.Node1.Y * 0.5 + r.Node2.Y * 0.5 });
				lines.Add(GetLine(r.Node1, r.Node2));
			}

			this.elements = elements;
			this.lines = lines;
		}

		GraphicsElement GetElement(NodeLayout node)
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

		GraphicsLine GetLine(NodeLayout node1, NodeLayout node2)
		{
			return new GraphicsLine() { X1 = node1.X, Y1 = node1.Y, X2 = node2.X, Y2 = node2.Y, Thickness = 2 };
		}

		public Tuple<List<GraphicsElement>, List<GraphicsLine>> Draw()
		{
			return new Tuple<List<GraphicsElement>, List<GraphicsLine>>(elements, lines);
		}
	}
}
