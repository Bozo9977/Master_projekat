using Common.DataModel;
using Common.GDA;
using System.Collections.Generic;

namespace CalculationEngine
{
	public class Node
	{
		public long GID { get; private set; }

		public Node(long gid)
		{
			GID = gid;
		}
	}

	public class Branch
	{
		public Node Node1 { get; private set; }
		public Node Node2 { get; private set; }
		public long GID { get; private set; }

		public Branch(Node node1, Node node2, long gid)
		{
			Node1 = node1;
			Node2 = node2;
			GID = gid;
		}
	}

	public class TopologyGraph
	{
		List<Branch> branches;

		public TopologyGraph(Dictionary<DMSType, Dictionary<long, IdentifiedObject>> containers)
		{

		}
	}
}
