﻿using Common.GDA;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.DataModel
{
	public class PowerTransformerDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
	}

	public class PowerTransformer : Equipment
	{
		public List<long> TransformerWindings { get; private set; }

		public PowerTransformer()
		{
			TransformerWindings = new List<long>();
		}

		public PowerTransformer(PowerTransformer p) : base(p)
		{
			TransformerWindings = new List<long>(p.TransformerWindings);
		}

		public PowerTransformer(PowerTransformerDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			TransformerWindings = new List<long>();
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS:
					return new ReferencesProperty(ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS, TransformerWindings);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p, bool force = false)
		{
			switch(p.Id)
			{
				case ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS:
					if(force)
					{
						TransformerWindings = ((ReferencesProperty)p).Value;
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
				case ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER:
					if(TransformerWindings.Contains(sourceGID))
						return false;

					TransformerWindings.Add(sourceGID);
					return true;
			}

			return base.AddTargetReference(sourceProperty, sourceGID);
		}

		public override bool RemoveTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER:
					return TransformerWindings.Remove(sourceGID);
			}

			return base.RemoveTargetReference(sourceProperty, sourceGID);
		}

		public override void GetTargetReferences(Dictionary<ModelCode, List<long>> dst)
		{
			dst[ModelCode.POWERTRANSFORMER_TRANSFORMERWINDINGS] = new List<long>(TransformerWindings);
			base.GetTargetReferences(dst);
		}

		public override bool IsReferenced()
		{
			return TransformerWindings.Count > 0 || base.IsReferenced();
		}

		public override IdentifiedObject Clone()
		{
			return new PowerTransformer(this);
		}
		public override object ToDBEntity()
		{
			return new PowerTransformerDBModel() { GID = GID, MRID = MRID, Name = Name };
		}

		public override void GetEntitiesToValidate(Func<long, IdentifiedObject> entityGetter, HashSet<long> dst)
		{
			foreach(long w in TransformerWindings)
				dst.Add(w);

			base.GetEntitiesToValidate(entityGetter, dst);
		}
	}
}
