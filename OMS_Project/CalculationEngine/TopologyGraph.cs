using Common;
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
		public int AdjacentOffset { get; set; }
		public int AdjacentCount { get; set; }

		public Node(IdentifiedObject io, int adjacentOffset, int adjacentCount)
		{
			IO = io;
			AdjacentOffset = adjacentOffset;
			AdjacentCount = adjacentCount;
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
		IReadOnlyDictionary<long, bool> markedSwitchStates;
		List<DailyLoadProfile> loadProfiles;
		DateTime t;

		public TopologyGraph(Dictionary<DMSType, Dictionary<long, IdentifiedObject>> containers, IReadOnlyDictionary<long, float> analogs, IReadOnlyDictionary<long, int> discretes, IReadOnlyDictionary<long, bool> markedSwitchStates, List<DailyLoadProfile> loadProfiles)
		{
			subGraphs = new List<Node>();
			adjacency = new List<Node>();
			this.containers = containers;
			this.analogs = analogs;
			this.discretes = discretes;
			this.markedSwitchStates = markedSwitchStates;
			this.loadProfiles = loadProfiles;
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
				node.AdjacentOffset = adjacency.Count;
				adjacency.Add(tuple.Item1);
				++node.AdjacentCount;

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

						Node adjacentNode;

						if(!visited.TryGetValue(ceGID, out adjacentNode))
						{
							DMSType ceType = ModelCodeHelper.GetTypeFromGID(ceGID);
							IdentifiedObject ce;

							if(!containers[ceType].TryGetValue(ceGID, out ce))
								continue;

							adjacentNode = new Node(ce, 0, 0);
							queue.Enqueue(new Tuple<Node, Node>(node, adjacentNode));
							visited.Add(ceGID, adjacentNode);
						}

						adjacency.Add(adjacentNode);
						++node.AdjacentCount;
					}
				}
				else if(type == DMSType.TransformerWinding)
				{
					TransformerWinding tw = node.IO as TransformerWinding;
					PowerTransformer pt = Get(tw.PowerTransformer) as PowerTransformer;

					if(pt == null)
						continue;

					foreach(long twGID in pt.TransformerWindings)
					{
						if(twGID == tw.GID)
							continue;

						Node adjacentNode;

						if(!visited.TryGetValue(twGID, out adjacentNode))
						{
							TransformerWinding twAdjacent = Get(twGID) as TransformerWinding;

							if(twAdjacent == null)
								continue;

							adjacentNode = new Node(twAdjacent, 0, 0);
							queue.Enqueue(new Tuple<Node, Node>(node, adjacentNode));
							visited.Add(twGID, adjacentNode);
						}

						adjacency.Add(adjacentNode);
						++node.AdjacentCount;
						break;
					}

					foreach(long tGID in tw.Terminals)
					{
						IdentifiedObject terminal;

						if(!terminals.TryGetValue(tGID, out terminal))
							continue;

						long cNodeGID = ((Terminal)terminal).ConnectivityNode;

						if(cNodeGID == 0)
							continue;

						Node adjacentNode;

						if(!visited.TryGetValue(cNodeGID, out adjacentNode))
						{
							IdentifiedObject cNode;

							if(!cNodes.TryGetValue(cNodeGID, out cNode))
								continue;

							adjacentNode = new Node(cNode, 0, 0);
							queue.Enqueue(new Tuple<Node, Node>(node, adjacentNode));
							visited.Add(cNodeGID, adjacentNode);
						}

						adjacency.Add(adjacentNode);
						++node.AdjacentCount;
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

						Node adjacentNode;

						if(!visited.TryGetValue(cNodeGID, out adjacentNode))
						{
							IdentifiedObject cNode;

							if(!cNodes.TryGetValue(cNodeGID, out cNode))
								continue;

							adjacentNode = new Node(cNode, 0, 0);
							queue.Enqueue(new Tuple<Node, Node>(node, adjacentNode));
							visited.Add(cNodeGID, adjacentNode);
						}

						adjacency.Add(adjacentNode);
						++node.AdjacentCount;
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

					if(energization != EEnergization.NotEnergized && ModelCodeHelper.ModelCodeClassIsSubClassOf(dmsTypeToModelCodeMap[ModelCodeHelper.GetTypeFromGID(gid)], ModelCode.SWITCH) && GetSwitchState((Switch)node.Item1.IO))
					{
						energization = EEnergization.NotEnergized;
					}

					if(energization == EEnergization.NotEnergized)
						continue;

					if(energization == EEnergization.Energized)
					{
						for(int j = node.Item1.AdjacentOffset; j < node.Item1.AdjacentOffset + node.Item1.AdjacentCount; ++j)
						{
							Node adjacentNode = adjacency[j];

							if(adjacentNode == null)
								continue;

							long adjacentGID = adjacentNode.IO.GID;
							Tuple<long, long> line = gid <= adjacentGID ? new Tuple<long, long>(gid, adjacentGID) : new Tuple<long, long>(adjacentGID, gid);

							if(visitedEnergized.Contains(line))
								continue;

							stack.Push(new Tuple<Node, EEnergization>(adjacentNode, EEnergization.Energized));

							visitedEnergized.Add(line);
							visitedUnknown.Remove(line);
						}
					}
					else
					{
						for(int j = node.Item1.AdjacentOffset; j < node.Item1.AdjacentOffset + node.Item1.AdjacentCount; ++j)
						{
							Node adjacentNode = adjacency[j];

							if(adjacentNode == null)
								continue;

							long adjacentGID = adjacentNode.IO.GID;
							Tuple<long, long> line = gid <= adjacentGID ? new Tuple<long, long>(gid, adjacentGID) : new Tuple<long, long>(adjacentGID, gid);

							if(visitedEnergized.Contains(line) || visitedUnknown.Contains(line))
								continue;

							stack.Push(new Tuple<Node, EEnergization>(adjacentNode, EEnergization.Unknown));

							visitedUnknown.Add(line);
						}
					}
				}

				sourcesEnergization.Add(new Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>(subGraphs[i].IO.GID, new List<Tuple<long, long>>(visitedEnergized), new List<Tuple<long, long>>(visitedUnknown)));
			}

			return sourcesEnergization;
		}

		IdentifiedObject Get(long gid)
		{
			IdentifiedObject io;
			Dictionary<long, IdentifiedObject> container;
			return containers.TryGetValue(ModelCodeHelper.GetTypeFromGID(gid), out container) && container.TryGetValue(gid, out io) ? io : null;
		}

		public List<KeyValuePair<long, LoadFlowResult>> CalculateLoadFlow()
		{
			t = DateTime.Now;
			List<KeyValuePair<long, LoadFlowResult>> result = new List<KeyValuePair<long, LoadFlowResult>>();

			for(int i = 0; i < subGraphs.Count; ++i)
			{
				Node root = subGraphs[i];
				CalculateLoadFlowForSubGraph(root, result);
			}

			return result;
		}

		const int maxIterations = 100;
		const double voltageRelativeDelta = 0.01;

		void CalculateLoadFlowForSubGraph(Node source, List<KeyValuePair<long, LoadFlowResult>> result)
		{
			Dictionary<long, LoadFlowResult> lf = new Dictionary<long, LoadFlowResult>();
			
			EnergySource es = source.IO as EnergySource;
			if(es == null)
				return;

			Complex u1 = GetVoltageFromEnergySource(es);

			if(u1.IsNaN())
				return;

			Dictionary<long, Complex> currents = new Dictionary<long, Complex>();

			for(int iteration = 0; iteration < maxIterations; ++iteration)
			{
				double maxVoltageRelativeDelta = 0;

				Stack<Triple<Node, int, List<Complex>>> stack = new Stack<Triple<Node, int, List<Complex>>>();
				stack.Push(new Triple<Node, int, List<Complex>>(source, 0, new List<Complex>(source.AdjacentCount - 1)));

				HashSet<long> visited = new HashSet<long>();

				while(stack.Count > 0)
				{
					Triple<Node, int, List<Complex>> triple = stack.Pop();
					Node node = triple.First;
					int childrenPos = triple.Second;
					Func<Node, IEnumerable<Complex>, Dictionary<long, LoadFlowResult>, Complex> currentFunction = CalculateCurrentDefault;

					visited.Add(node.IO.GID);
					DMSType type = ModelCodeHelper.GetTypeFromGID(node.IO.GID);

					switch(type)
					{
						case DMSType.Recloser:
							continue;

						case DMSType.ConnectivityNode:
						{
							LoadFlowResult lfr;

							if(!lf.TryGetValue(node.IO.GID, out lfr))
							{
								lfr = new LoadFlowResult();
								lfr.Add(new LoadFlowResultItem(u1.X, LoadFlowResultType.UR));
								lfr.Add(new LoadFlowResultItem(u1.Y, LoadFlowResultType.UI));
								lf[node.IO.GID] = lfr;
							}
						}
						break;

						case DMSType.EnergyConsumer:
						{
							currentFunction = CalculateCurrentForEnergyConsumer;

							LoadFlowResult lfr;

							if(!lf.TryGetValue(node.IO.GID, out lfr))
							{
								Complex s = GetPowerForEnergyConsumer(node.IO as EnergyConsumer);

								lfr = new LoadFlowResult();
								lfr.Add(new LoadFlowResultItem(s.X, LoadFlowResultType.SR));
								lfr.Add(new LoadFlowResultItem(s.Y, LoadFlowResultType.SI));
								lf[node.IO.GID] = lfr;
							}
						}
						break;

						case DMSType.Breaker:
						case DMSType.Disconnector:
						{
							currentFunction = CalculateCurrentForSwitch;

							if(childrenPos == 0)
							{
								double maxDelta = CalculateVoltageForSwitch(node, lf, currents);

								if(maxDelta > maxVoltageRelativeDelta)
									maxVoltageRelativeDelta = maxDelta;
							}
						}
						break;

						case DMSType.DistributionGenerator:
						{
							currentFunction = CalculateCurrentForDistributionGenerator;
						}
						break;

						case DMSType.TransformerWinding:
						{
							currentFunction = CalculateCurrentForTransformerWinding;

							if(childrenPos == 0)
							{
								double maxDelta = CalculateVoltageForTransformerWinding(node, lf, currents);

								if(maxDelta > maxVoltageRelativeDelta)
									maxVoltageRelativeDelta = maxDelta;
							}
						}
						break;

						case DMSType.ACLineSegment:
						{
							currentFunction = CalculateCurrentForACLineSegment;

							if(childrenPos == 0)
							{
								double maxDelta = CalculateVoltageForACLineSegment(node, lf, currents);

								if(maxDelta > maxVoltageRelativeDelta)
									maxVoltageRelativeDelta = maxDelta;
							}
						}
						break;
					}

					Node childNode = null;

					for(int i = node.AdjacentOffset + childrenPos; i < node.AdjacentOffset + node.AdjacentCount; ++i)
					{
						Node adjacentNode = adjacency[i];

						if(adjacentNode == null || visited.Contains(adjacentNode.IO.GID))
							continue;

						childNode = adjacentNode;
						break;
					}

					if(childNode != null)
					{
						stack.Push(new Triple<Node, int, List<Complex>>(node, childrenPos + 1, triple.Third));
						stack.Push(new Triple<Node, int, List<Complex>>(childNode, 0, new List<Complex>(childNode.AdjacentCount - 1)));
						continue;
					}

					if(stack.Count > 0)
					{
						Complex current = currentFunction(node, triple.Third, lf);
						stack.Peek().Third.Add(current);
						currents[node.IO.GID] = current;
					}
				}

				if(maxVoltageRelativeDelta < voltageRelativeDelta)
					break;
			}

			result.AddRange(lf);
		}

		double CalculateVoltageForACLineSegment(Node node, Dictionary<long, LoadFlowResult> lf, Dictionary<long, Complex> currents)
		{
			Complex current;

			if(!currents.TryGetValue(node.IO.GID, out current))
				return double.MaxValue;

			ACLineSegment segment = node.IO as ACLineSegment;

			List<Node> nodes = new List<Node>(2);

			for(int i = node.AdjacentOffset; i < node.AdjacentOffset + node.AdjacentCount; ++i)
			{
				Node n = adjacency[i];

				if(!nodes.Contains(n) && ModelCodeHelper.GetTypeFromGID(n.IO.GID) == DMSType.ConnectivityNode)
				{
					nodes.Add(n);
				}
			}

			if(nodes.Count != 2)
				return 0;

			LoadFlowResult lfr1;

			if(!lf.TryGetValue(nodes[0].IO.GID, out lfr1))
				return 0;

			Complex u1 = new Complex(lfr1.Get(LoadFlowResultType.UR), lfr1.Get(LoadFlowResultType.UI));
			Complex u2 = u1.Subtract(new Complex(segment.PerLengthPhaseResistance * segment.Length, segment.PerLengthPhaseReactance * segment.Length).Multiply(current));

			LoadFlowResult lfr2;
			double relDelta = double.MaxValue;

			if(!lf.TryGetValue(nodes[1].IO.GID, out lfr2))
			{
				lfr2 = new LoadFlowResult();
				lf[nodes[1].IO.GID] = lfr2;
			}
			else
			{
				relDelta = GetVoltageRelativeDifference(new Complex(lfr2.Get(LoadFlowResultType.UR), lfr2.Get(LoadFlowResultType.UI)), u2);
			}

			lfr2.Remove(LoadFlowResultType.UR);
			lfr2.Remove(LoadFlowResultType.UI);

			lfr2.Add(new LoadFlowResultItem(u2.X, LoadFlowResultType.UR));
			lfr2.Add(new LoadFlowResultItem(u2.Y, LoadFlowResultType.UI));

			return relDelta;
		}

		double GetVoltageRelativeDifference(Complex u1, Complex u2)
		{
			double u1m = u1.Magnitude();
			return Math.Abs((u2.Magnitude() - u1m) / u1m);
		}

		double CalculateVoltageForTransformerWinding(Node node, Dictionary<long, LoadFlowResult> lf, Dictionary<long, Complex> currents)
		{
			TransformerWinding tw = node.IO as TransformerWinding;

			if(IsTransformerWindingPrimary(tw))
				return 0;

			if(!currents.ContainsKey(node.IO.GID))
				return double.MaxValue;

			PowerTransformer pw = Get(tw.PowerTransformer) as PowerTransformer;
			TransformerWinding tw1 = null;

			for(int i = 0; i < pw.TransformerWindings.Count; ++i)
			{
				long twGid = pw.TransformerWindings[i];

				if(twGid != tw.GID)
				{
					tw1 = Get(twGid) as TransformerWinding;
					break;
				}
			}

			long node1Gid = (Get(tw1.Terminals[0]) as Terminal).ConnectivityNode;
			long node2Gid = (Get(tw.Terminals[0]) as Terminal).ConnectivityNode;

			LoadFlowResult lfr1;

			if(!lf.TryGetValue(node1Gid, out lfr1))
				return 0;

			Complex u1 = new Complex(lfr1.Get(LoadFlowResultType.UR), lfr1.Get(LoadFlowResultType.UI));
			Complex u2 = u1.Scale(1.0 / GetPowerTransformerRatio(pw));

			LoadFlowResult lfr2;
			double relDelta = double.MaxValue;

			if(!lf.TryGetValue(node2Gid, out lfr2))
			{
				lfr2 = new LoadFlowResult();
				lf[node2Gid] = lfr2;
			}
			else
			{
				relDelta = GetVoltageRelativeDifference(new Complex(lfr2.Get(LoadFlowResultType.UR), lfr2.Get(LoadFlowResultType.UI)), u2);
			}

			lfr2.Remove(LoadFlowResultType.UR);
			lfr2.Remove(LoadFlowResultType.UI);

			lfr2.Add(new LoadFlowResultItem(u2.X, LoadFlowResultType.UR));
			lfr2.Add(new LoadFlowResultItem(u2.Y, LoadFlowResultType.UI));

			return relDelta;
		}

		double CalculateVoltageForSwitch(Node node, Dictionary<long, LoadFlowResult> lf, Dictionary<long, Complex> currents)
		{
			if(!currents.ContainsKey(node.IO.GID))
				return double.MaxValue;

			List<Node> nodes = new List<Node>(2);

			for(int i = node.AdjacentOffset; i < node.AdjacentOffset + node.AdjacentCount; ++i)
			{
				Node n = adjacency[i];

				if(!nodes.Contains(n) && ModelCodeHelper.GetTypeFromGID(n.IO.GID) == DMSType.ConnectivityNode)
				{
					nodes.Add(n);
				}
			}

			if(nodes.Count != 2)
				return 0;

			LoadFlowResult lfr1;

			if(!lf.TryGetValue(nodes[0].IO.GID, out lfr1))
				return 0;

			Complex u1 = new Complex(lfr1.Get(LoadFlowResultType.UR), lfr1.Get(LoadFlowResultType.UI));
			Complex u2 = GetSwitchState(node.IO as Switch) ? new Complex(0, 0) : u1;

			LoadFlowResult lfr2;
			double relDelta = double.MaxValue;

			if(!lf.TryGetValue(nodes[1].IO.GID, out lfr2))
			{
				lfr2 = new LoadFlowResult();
				lf[nodes[1].IO.GID] = lfr2;
			}
			else
			{
				relDelta = GetVoltageRelativeDifference(new Complex(lfr2.Get(LoadFlowResultType.UR), lfr2.Get(LoadFlowResultType.UI)), u2);
			}

			lfr2.Remove(LoadFlowResultType.UR);
			lfr2.Remove(LoadFlowResultType.UI);

			lfr2.Add(new LoadFlowResultItem(u2.X, LoadFlowResultType.UR));
			lfr2.Add(new LoadFlowResultItem(u2.Y, LoadFlowResultType.UI));

			return relDelta;
		}

		Complex GetVoltageFromEnergySource(EnergySource es)
		{
			float re = float.NaN;
			float im = float.NaN;

			for(int i = 0; i < es.Measurements.Count; ++i)
			{
				Analog analog = Get(es.Measurements[i]) as Analog;

				if(analog == null)
					continue;

				switch(analog.MeasurementType)
				{
					case MeasurementType.VoltageR:
						if(!analogs.TryGetValue(analog.GID, out re))
							re = analog.NormalValue;

						break;

					case MeasurementType.VoltageI:
						if(!analogs.TryGetValue(analog.GID, out im))
							im = analog.NormalValue;

						break;
				}
			}

			if(float.IsNaN(re) || float.IsNaN(im))
			{
				BaseVoltage bv = Get(es.BaseVoltage) as BaseVoltage;
				
				if(bv != null)
				{
					re = bv.NominalVoltage;
					im = 0;
				}
			}

			return new Complex(re, im);
		}

		Complex CalculateCurrentDefault(Node node, IEnumerable<Complex> childCurrents, Dictionary<long, LoadFlowResult> lf)
		{
			Complex i = new Complex(0, 0);

			foreach(Complex ci in childCurrents)
			{
				if(ci.IsNaN())
					continue;

				i = i.Add(ci);
			}

			return i;
		}

		Complex CalculateCurrentForEnergyConsumer(Node node, IEnumerable<Complex> childCurrents, Dictionary<long, LoadFlowResult> lf)
		{
			EnergyConsumer ec = node.IO as EnergyConsumer;

			if(ec == null || ec.Terminals.Count != 1)
				return Complex.NaN();

			Terminal t = Get(ec.Terminals[0]) as Terminal;

			if(t == null)
				return Complex.NaN();

			LoadFlowResult lfr;

			if(!lf.TryGetValue(t.ConnectivityNode, out lfr))
				return Complex.NaN();

			double ur = lfr.Get(LoadFlowResultType.UR);
			double ui = lfr.Get(LoadFlowResultType.UI);

			return GetPowerForEnergyConsumer(ec).Divide(new Complex(ur, -ui));
		}

		Complex GetPowerForEnergyConsumer(EnergyConsumer ec)
		{
			float re = float.NaN;
			float im = float.NaN;

			for(int i = 0; i < ec.Measurements.Count; ++i)
			{
				Analog analog = Get(ec.Measurements[i]) as Analog;

				if(analog == null)
					continue;

				switch(analog.MeasurementType)
				{
					case MeasurementType.ActivePower:
						if(!analogs.TryGetValue(analog.GID, out re))
							re = analog.NormalValue;

						break;

					case MeasurementType.ReactivePower:
						if(!analogs.TryGetValue(analog.GID, out im))
							im = analog.NormalValue;

						break;
				}
			}

			if(float.IsNaN(re) || float.IsNaN(im))
			{
				if(!ReadEnergyConsumerPowerFromLoadProfile(ec, out re, out im))
				{
					re = ec.PFixed;
					im = ec.QFixed;
				}
			}

			return new Complex(re, im);
		}

		bool ReadEnergyConsumerPowerFromLoadProfile(EnergyConsumer ec, out float re, out float im)
		{
			re = float.NaN;
			im = float.NaN;

			DailyLoadProfile loadProfile = loadProfiles.Find(x => x.ConsumerClass == ec.ConsumerClass);

			if(loadProfile == null)
				return false;

			float pu = loadProfile.Get(t.Hour, t.Minute);

			if(float.IsNaN(pu))
				return false;

			re = pu * ec.PFixed;
			im = pu * ec.QFixed;

			return true;
		}

		Complex CalculateCurrentForSwitch(Node node, IEnumerable<Complex> childCurrents, Dictionary<long, LoadFlowResult> lf)
		{
			return GetSwitchState(node.IO as Switch) ? new Complex(0, 0) : CalculateCurrentDefault(node, childCurrents, lf);
		}

		bool GetSwitchState(Switch s)
		{
			Discrete discrete = null;

			for(int i = 0; i < s.Measurements.Count; ++i)
			{
				Discrete d = Get(s.Measurements[i]) as Discrete;

				if(d == null || d.MeasurementType != MeasurementType.SwitchState)
					continue;

				discrete = d;
				break;
			}

			int state;

			if(discrete == null || !discretes.TryGetValue(discrete.GID, out state))
			{
				bool open;
				return markedSwitchStates.TryGetValue(s.GID, out open) ? open : s.NormalOpen;
			}
			
			return state != 0;
		}

		Complex CalculateCurrentForDistributionGenerator(Node node, IEnumerable<Complex> childCurrents, Dictionary<long, LoadFlowResult> lf)
		{
			DistributionGenerator dg = node.IO as DistributionGenerator;

			if(dg == null || dg.Terminals.Count != 1)
				return Complex.NaN();

			Terminal t = Get(dg.Terminals[0]) as Terminal;

			if(t == null)
				return Complex.NaN();

			LoadFlowResult lfr;

			if(!lf.TryGetValue(t.ConnectivityNode, out lfr))
				return Complex.NaN();

			double ur = lfr.Get(LoadFlowResultType.UR);
			double ui = lfr.Get(LoadFlowResultType.UI);
			Complex i = GetPowerForDistributionGenerator(dg).Divide(new Complex(ur, -ui));

			return i;
		}

		Complex GetPowerForDistributionGenerator(DistributionGenerator dg)
		{
			float re = float.NaN;
			float im = float.NaN;

			for(int i = 0; i < dg.Measurements.Count; ++i)
			{
				Analog analog = Get(dg.Measurements[i]) as Analog;

				if(analog == null)
					continue;

				switch(analog.MeasurementType)
				{
					case MeasurementType.ActivePower:
						if(!analogs.TryGetValue(analog.GID, out re))
							re = analog.NormalValue;

						break;

					case MeasurementType.ReactivePower:
						if(!analogs.TryGetValue(analog.GID, out im))
							im = analog.NormalValue;

						break;
				}
			}

			if(float.IsNaN(re) || float.IsNaN(im))
			{
				float ratedS = dg.RatedPower;
				float ratedP = ratedS * dg.RatedCosPhi;
				float ratedQ = (float)(ratedS * Math.Sqrt(1 - dg.RatedCosPhi * dg.RatedCosPhi));

				re = ratedP;
				im = ratedQ;
			}

			return new Complex(-re, -im);
		}

		Complex CalculateCurrentForTransformerWinding(Node node, IEnumerable<Complex> childCurrents, Dictionary<long, LoadFlowResult> lf)
		{
			TransformerWinding tw = node.IO as TransformerWinding;

			if(tw == null)
				return Complex.NaN();

			if(!IsTransformerWindingPrimary(tw))
				return CalculateCurrentDefault(node, childCurrents, lf);

			return CalculateCurrentDefault(node, childCurrents, lf).Scale(GetPowerTransformerRatio(Get(tw.PowerTransformer) as PowerTransformer));
		}

		bool IsTransformerWindingPrimary(TransformerWinding tw)
		{
			PowerTransformer pt = Get(tw.PowerTransformer) as PowerTransformer;
			TransformerWinding other = null;

			for(int i = 0; i < pt.TransformerWindings.Count; ++i)
			{
				long otherGID = pt.TransformerWindings[i];

				if(otherGID == tw.GID)
					continue;

				TransformerWinding tw2 = Get(otherGID) as TransformerWinding;

				if(tw2 != null)
				{
					other = tw2;
					break;
				}
			}

			BaseVoltage bv1 = Get(tw.BaseVoltage) as BaseVoltage;
			BaseVoltage bv2 = Get(other.BaseVoltage) as BaseVoltage;

			return bv1.NominalVoltage > bv2.NominalVoltage;
		}

		Pair<double, double> MinMax(double x, double y)
		{
			return x > y ? new Pair<double, double>(y, x) : new Pair<double, double>(x, y);
		}

		double GetPowerTransformerRatio(PowerTransformer pt)
		{
			if(pt.TransformerWindings.Count != 2)
				return 1;

			TransformerWinding tw1 = Get(pt.TransformerWindings[0]) as TransformerWinding;
			TransformerWinding tw2 = Get(pt.TransformerWindings[1]) as TransformerWinding;
			BaseVoltage bv1 = Get(tw1.BaseVoltage) as BaseVoltage;
			BaseVoltage bv2 = Get(tw2.BaseVoltage) as BaseVoltage;
			Pair<double, double> u = MinMax(bv1.NominalVoltage, bv2.NominalVoltage);
			RatioTapChanger rt = null;

			if(tw1.RatioTapChanger.Count == 1)
			{
				rt = Get(tw1.RatioTapChanger[0]) as RatioTapChanger;
			}
			else if(tw2.RatioTapChanger.Count == 1)
			{
				rt = Get(tw2.RatioTapChanger[0]) as RatioTapChanger;
			}

			return rt != null ? (u.Second + (GetRatioTapChangerStep(rt) - rt.NominalStep) * rt.VoltageStep) / u.First : u.Second / u.First;
		}

		int GetRatioTapChangerStep(RatioTapChanger rt)
		{
			for(int i = 0; i < rt.Measurements.Count; ++i)
			{
				Discrete discrete = Get(rt.Measurements[i]) as Discrete;
				int step;

				if(discrete != null && discrete.MeasurementType == MeasurementType.TapChangerPosition && discretes.TryGetValue(discrete.GID, out step))
					return step;
			}

			return rt.NominalStep;
		}

		Complex CalculateCurrentForACLineSegment(Node node, IEnumerable<Complex> childCurrents, Dictionary<long, LoadFlowResult> lf)
		{
			Complex result = CalculateCurrentDefault(node, childCurrents, lf);
			
			LoadFlowResult lfr;

			if(!lf.TryGetValue(node.IO.GID, out lfr))
			{
				lfr = new LoadFlowResult();
				lf[node.IO.GID] = lfr;
			}

			lfr.Remove(LoadFlowResultType.IR);
			lfr.Remove(LoadFlowResultType.II);

			lfr.Add(new LoadFlowResultItem(result.X, LoadFlowResultType.IR));
			lfr.Add(new LoadFlowResultItem(result.Y, LoadFlowResultType.II));

			List<Node> nodes = new List<Node>(2);

			for(int i = node.AdjacentOffset; i < node.AdjacentOffset + node.AdjacentCount; ++i)
			{
				Node n = adjacency[i];

				if(!nodes.Contains(n) && ModelCodeHelper.GetTypeFromGID(n.IO.GID) == DMSType.ConnectivityNode)
				{
					nodes.Add(n);
				}
			}

			if(nodes.Count != 2)
				return result;

			LoadFlowResult lfr1;
			LoadFlowResult lfr2;

			if(!lf.TryGetValue(nodes[0].IO.GID, out lfr1) || !lf.TryGetValue(nodes[1].IO.GID, out lfr2))
				return result;

			ACLineSegment segment = node.IO as ACLineSegment;
			Complex u1 = new Complex(lfr1.Get(LoadFlowResultType.UR), lfr1.Get(LoadFlowResultType.UI));
			Complex u2 =  new Complex(lfr2.Get(LoadFlowResultType.UR), lfr2.Get(LoadFlowResultType.UI));
			Complex s = u1.Subtract(u2).Multiply(new Complex(result.X, -result.Y));
		
			lfr.Remove(LoadFlowResultType.SR);
			lfr.Remove(LoadFlowResultType.SI);
			
			lfr.Add(new LoadFlowResultItem(s.X, LoadFlowResultType.SR));
			lfr.Add(new LoadFlowResultItem(s.Y, LoadFlowResultType.SI));
			
			return result;
		}
	}
}
