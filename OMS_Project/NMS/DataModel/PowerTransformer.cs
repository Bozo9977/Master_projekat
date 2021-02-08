using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class PowerTransformer : Equipment
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
	}
}
