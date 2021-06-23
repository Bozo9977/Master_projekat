using Common;
using Common.CalculationEngine;
using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GUI
{
	public interface IElementLayout
	{
		uint X { get; }
		uint Y { get; }
		IdentifiedObject IO { get; }
		EEnergization Energization { get; }
	}

	public class NodeLayout : IElementLayout
	{
		public uint X { get; set; }
		public uint Y { get; set; }
		//public uint SubtreeDepth { get; set; }
		public Node Node { get; private set; }
		public NodeLayout Parent { get; private set; }
		public List<NodeLayout> Children { get; private set; }
		public IdentifiedObject IO { get { return Node.io; } }
		public EEnergization Energization { get; set; }

		public NodeLayout(NodeLayout parent, Node node)
		{
			Node = node;
			Parent = parent;
			Children = new List<NodeLayout>();
		}
	}

	public class RecloserLayout : IElementLayout
	{
		public uint X { get { return Node1.X / 2 + Node2.X / 2 + (Node1.X % 2 + Node2.X % 2) / 2; } }
		public uint Y { get { return Node1.Y / 2 + Node2.Y / 2 + (Node1.Y % 2 + Node2.Y % 2) / 2; } }
		public NodeLayout Node1 { get; private set; }
		public NodeLayout Node2 { get; private set; }
		public RecloserNode Recloser { get; private set; }
		public IdentifiedObject IO { get { return Recloser.io; } }
		public EEnergization Energization { get; set; }

		public RecloserLayout(RecloserNode r, NodeLayout node1, NodeLayout node2)
		{
			Node1 = node1;
			Node2 = node2;
			Recloser = r;
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
		NetworkModel networkModel;
		Topology topology;
		Measurements measurements;

		bool networkModelChanged;
		bool topologyChanged;
		bool loadFlowChanged;

		Dictionary<DMSType, ModelCode> dmsTypeToModelCodeMap;

		NodeLayout root;
		List<RecloserLayout> reclosers;

		List<GraphicsElement> elements;
		List<GraphicsLine> lines;
		List<GraphicsText> loadFlows;

		public NetworkModelDrawing()
		{
			networkModelChanged = true;
			topologyChanged = true;
			loadFlowChanged = true;
			elements = new List<GraphicsElement>(0);
			lines = new List<GraphicsLine>(0);
			loadFlows = new List<GraphicsText>();
			dmsTypeToModelCodeMap = ModelResourcesDesc.GetTypeToModelCodeMap();
		}

		public NetworkModel NetworkModel
		{
			get
			{
				return networkModel;
			}
			set
			{
				networkModel = value;
				networkModelChanged = true;
			}
		}

		public Topology Topology
		{
			get
			{
				return topology;
			}
			set
			{
				topology = value;
				topologyChanged = true;
			}
		}

		public Measurements Measurements
		{
			get
			{
				return measurements;
			}
			set
			{
				measurements = value;
			}
		}

		public void UpdateLoadFlow()
		{
			loadFlowChanged = true;
		}

		void Layout()
		{
			if(!networkModelChanged || networkModel == null)
				return;

			networkModelChanged = false;

			Tuple<List<Node>, List<RecloserNode>> model = networkModel.GetTreesAndReclosers();

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
				RecloserLayout r = new RecloserLayout(rn, recloserConNodes[rn.node1], recloserConNodes[rn.node2]);
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
						node.X = x += ModelCodeHelper.GetTypeFromGID(node.IO.GID) == DMSType.EnergyConsumer ? 2 : 1;
					}

					node = node.Parent;
				}
				while(node != null);
			}

			this.root = root;
			this.reclosers = reclosers;

			topologyChanged = true;
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
							depths[i] = Math.Min(depths[i], r.Depth);
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
							depths.Insert(maxi + 1, Math.Min(r.Depth, maxd));

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
							depths.Insert(maxi, Math.Min(r.Depth, maxd));

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
			if(!topologyChanged || topology == null || measurements == null || root == null)
				return;

			topologyChanged = false;

			List<GraphicsElement> elements = new List<GraphicsElement>();
			List<GraphicsLine> lines = new List<GraphicsLine>();

			foreach(NodeLayout tree in root.Children)
			{
				Stack<NodeLayout> stack = new Stack<NodeLayout>();
				stack.Push(tree);

				while(stack.Count > 0)
				{
					NodeLayout node = stack.Pop();
					elements.Add(new GraphicsElement(node, GetNodeColor(node.IO)));

					foreach(NodeLayout child in node.Children)
					{
						lines.Add(new GraphicsLine(node, child, GetLineColor(node.IO, child.IO)));
						stack.Push(child);
					}
				}
			}

			foreach(RecloserLayout r in reclosers)
			{
				elements.Add(new GraphicsElement(r, GetNodeColor(r.IO)));
				lines.Add(new GraphicsLine(r.Node1, r, GetLineColor(r.Node1.IO, r.IO)));
				lines.Add(new GraphicsLine(r.Node2, r, GetLineColor(r.Node2.IO, r.IO)));
			}

			this.elements = elements;
			this.lines = lines;
		}

		const double million = 1000000;
		const double thousand = 1000;

		string GetLoadFlowItemText(double r, double i, string unit)
		{
			if(Math.Abs(r) > million || Math.Abs(i) > million)
			{
				r /= million;
				i /= million;
				unit = " M" + unit;
			}
			else if(Math.Abs(r) > thousand || Math.Abs(i) > thousand)
			{
				r /= thousand;
				i /= thousand;
				unit = " k" + unit;
			}
			else
			{
				unit = " " + unit;
			}

			return r.ToString("0.0") + (i < 0 ? "" : "+") + i.ToString("0.0") + unit;
		}

		double ComplexLength(double x, double y)
		{
			return Math.Sqrt(x * x + y * y);
		}

		const double loadFlowXOffset = 0.3;
		const double loadFlowYOffset = 0.5;
		const double loadFlowFontSize = 0.22;

		void RedrawLoadFlow()
		{
			if(!loadFlowChanged || topology == null)
				return;

			loadFlowChanged = false;
			List<GraphicsText> loadFlows = new List<GraphicsText>();
			
			for(int i = 0; i < elements.Count; ++i)
			{
				GraphicsElement element = elements[i];

				if(element.IO == null)
					continue;

				/*LoadFlowResult lfResult = topology.GetLoadFlow(element.IO.GID);

				if(lfResult == null)
					continue;*/

				switch(ModelCodeHelper.GetTypeFromGID(element.IO.GID))
				{
					case DMSType.ConnectivityNode:
					{
						double ur = 200.12, ui = -100.52;

						GraphicsText gtu = new GraphicsText(element.X + loadFlowXOffset, 0, GetLoadFlowItemText(ur, ui, "V"), Brushes.Black, Brushes.Transparent, element, loadFlowFontSize);
						gtu.Y = element.Y - gtu.CalculateSize().Height / 2;
						loadFlows.Add(gtu);
					}
					break;

					case DMSType.ACLineSegment:
					{
						double ir = 200000.12, ii = 100000.52;
						double sr = 200000.12, si = 100000.52;

						bool abnormalCurrent = ComplexLength(ir, ii) > ((ACLineSegment)element.IO).RatedCurrent;
						GraphicsText gti = new GraphicsText(element.X + loadFlowXOffset, 0, GetLoadFlowItemText(ir, ii, "A"), abnormalCurrent ? Brushes.White : Brushes.Black, abnormalCurrent ? Brushes.DarkRed : Brushes.Transparent, element, loadFlowFontSize);
						GraphicsText gts = new GraphicsText(element.X + loadFlowXOffset, 0, GetLoadFlowItemText(sr, si, "VA"), Brushes.Black, Brushes.Transparent, element, loadFlowFontSize);
						Size gtiSize = gti.CalculateSize();
						Size gtsSize = gts.CalculateSize();

						gti.Y = element.Y - (gtiSize.Height + gtsSize.Height) / 2;
						gts.Y = gti.Y + gtiSize.Height;

						loadFlows.Add(gti);
						loadFlows.Add(gts);
					}
					break;

					case DMSType.EnergyConsumer:
					{
						double sr = -200000000.12, si = -100000000.52;
						GraphicsText gts = new GraphicsText(0, element.Y + loadFlowYOffset, GetLoadFlowItemText(sr, si, "VA"), Brushes.Black, Brushes.Transparent, element, loadFlowFontSize);
						gts.X = element.X - gts.CalculateSize().Width / 2;
						loadFlows.Add(gts);
					}
					break;
				}
			}

			this.loadFlows = loadFlows;
		}

		Brush GetNodeColor(IdentifiedObject io)
		{
			ModelCode mc;

			if(!dmsTypeToModelCodeMap.TryGetValue(ModelCodeHelper.GetTypeFromGID(io.GID), out mc))
				return Brushes.SlateGray;

			if(!ModelCodeHelper.ModelCodeClassIsSubClassOf(mc, ModelCode.SWITCH))
				return GetColor(topology.GetNodeEnergization(io.GID));

			Switch s = (Switch)io;

			foreach(long measGID in s.Measurements)
			{
				Measurement m = (Measurement)networkModel.Get(measGID);

				if(m == null)
					continue;

				if(m.MeasurementType == MeasurementType.SwitchState)
				{
					int value;

					if(!measurements.GetDiscreteInput(m.GID, out value))
						continue;

					if(value == 0)
					{
						return Brushes.Green;	//closed
					}
					else
					{
						return Brushes.Blue;	//open
					}
				}
			}

			return Brushes.SlateGray;
		}

		Brush GetLineColor(IdentifiedObject io1, IdentifiedObject io2)
		{
			return GetColor(topology.GetLineEnergization(io1.GID, io2.GID));
		}

		Brush GetColor(EEnergization energization)
		{
			switch(energization)
			{
				case EEnergization.Energized:
					return Brushes.Green;

				case EEnergization.NotEnergized:
					return Brushes.Blue;
			}

			return Brushes.SlateGray;
		}

		public Sequence<IGraphicsElement> Draw()
		{
			Layout();
			Redraw();
			RedrawLoadFlow();
			return new Sequence<IGraphicsElement>(new List<IReadOnlyList<IGraphicsElement>>(3) { lines, elements, loadFlows });
		}
	}
}
