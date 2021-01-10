using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class ConnectivityNode : IdentifiedObject
	{
		public List<long> Terminals { get; private set; }

		public ConnectivityNode() { }

		public ConnectivityNode(ConnectivityNode cn)
		{
			Terminals = new List<long>(cn.Terminals);
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.CONNECTIVITYNODE_TERMINALS:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.CONNECTIVITYNODE_TERMINALS:
					return new ReferencesProperty(ModelCode.CONNECTIVITYNODE_TERMINALS, Terminals);
			}

			return base.GetProperty(p);
		}

		public override bool AddTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.TERMINAL_CONNECTIVITYNODE:
					if(Terminals.Contains(sourceGID))
						return false;

					Terminals.Add(sourceGID);
					return true;
			}

			return base.AddTargetReference(sourceProperty, sourceGID);
		}

		public override bool RemoveTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.TERMINAL_CONNECTIVITYNODE:
					return Terminals.Remove(sourceGID);
			}

			return base.RemoveTargetReference(sourceProperty, sourceGID);
		}

		public override void GetTargetReferences(Dictionary<ModelCode, List<long>> dst)
		{
			dst[ModelCode.CONNECTIVITYNODE_TERMINALS] = new List<long>(Terminals);
			base.GetTargetReferences(dst);
		}

		public override bool IsReferenced()
		{
			return Terminals.Count > 0 || base.IsReferenced();
		}

		public override IdentifiedObject Clone()
		{
			return new ConnectivityNode(this);
		}
	}
}
