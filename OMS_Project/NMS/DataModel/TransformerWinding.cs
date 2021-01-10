using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class TransformerWinding : ConductingEquipment
	{
		public long PowerTransformer { get; private set; }
		public List<long> RatioTapChanger { get; private set; }

		public TransformerWinding() { }

		public TransformerWinding(TransformerWinding t) : base(t)
		{
			PowerTransformer = t.PowerTransformer;
			RatioTapChanger = new List<long>(t.RatioTapChanger);
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER:
				case ModelCode.TRANSFORMERWINDING_RATIOTAPCHANGER:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER:
					return new ReferenceProperty(ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER, PowerTransformer);
				case ModelCode.TRANSFORMERWINDING_RATIOTAPCHANGER:
					return new ReferencesProperty(ModelCode.TRANSFORMERWINDING_RATIOTAPCHANGER, RatioTapChanger);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER:
					PowerTransformer = ((ReferenceProperty)p).Value;
					return true;
			}

			return base.SetProperty(p);
		}

		public override bool AddTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.RATIOTAPCHANGER_TRANSFORMERWINDING:
					if(RatioTapChanger.Contains(sourceGID))
						return false;

					RatioTapChanger.Add(sourceGID);
					return true;
			}

			return base.AddTargetReference(sourceProperty, sourceGID);
		}

		public override bool RemoveTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			switch(sourceProperty)
			{
				case ModelCode.RATIOTAPCHANGER_TRANSFORMERWINDING:
					return RatioTapChanger.Remove(sourceGID);
			}

			return base.RemoveTargetReference(sourceProperty, sourceGID);
		}

		public override void GetTargetReferences(Dictionary<ModelCode, List<long>> dst)
		{
			dst[ModelCode.TRANSFORMERWINDING_RATIOTAPCHANGER] = new List<long>(RatioTapChanger);
			base.GetTargetReferences(dst);
		}

		public override bool IsReferenced()
		{
			return RatioTapChanger.Count > 0 || base.IsReferenced();
		}

		public override void GetSourceReferences(Dictionary<ModelCode, long> dst)
		{
			dst[ModelCode.TRANSFORMERWINDING_POWERTRANSFORMER] = PowerTransformer;
			base.GetSourceReferences(dst);
		}

		public override IdentifiedObject Clone()
		{
			return new TransformerWinding(this);
		}
	}
}
