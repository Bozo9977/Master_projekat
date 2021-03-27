using Common.GDA;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModel
{
	public class ConnectivityNodeDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
	}

	public class ConnectivityNode : IdentifiedObject
	{
		public List<long> Terminals { get; private set; }

		public ConnectivityNode()
		{
			Terminals = new List<long>();
		}

		public ConnectivityNode(ConnectivityNode c) : base(c)
		{
			Terminals = new List<long>(c.Terminals);
		}

		public ConnectivityNode(ConnectivityNodeDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			Terminals = new List<long>();
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

		public override bool SetProperty(Property p, bool force = false)
		{
			switch(p.Id)
			{
				case ModelCode.CONNECTIVITYNODE_TERMINALS:
					if(force)
					{
						Terminals = ((ReferencesProperty)p).Value;
						return true;
					}
					return false;
			}

			return base.SetProperty(p, force);
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

		public override object ToDBEntity()
		{
			return new ConnectivityNodeDBModel() { GID = GID, MRID = MRID, Name = Name };
		}
	}
}
