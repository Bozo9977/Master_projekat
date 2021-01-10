using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	class RatioTapChanger : TapChanger
	{
		public int NominalStep { get; private set; }
		public int StepCount { get; private set; }
		public float VoltageStep { get; private set; }
		public long TransformerWinding { get; private set; }

		public RatioTapChanger() { }

		public RatioTapChanger(RatioTapChanger r) : base(r)
		{
			NominalStep = r.NominalStep;
			StepCount = r.StepCount;
			VoltageStep = r.VoltageStep;
			TransformerWinding = r.TransformerWinding;
		}

		public override bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.RATIOTAPCHANGER_NOMINALSTEP:
				case ModelCode.RATIOTAPCHANGER_STEPCOUNT:
				case ModelCode.RATIOTAPCHANGER_VOLTAGESTEP:
				case ModelCode.RATIOTAPCHANGER_TRANSFORMERWINDING:
					return true;
			}

			return base.HasProperty(p);
		}

		public override Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.RATIOTAPCHANGER_NOMINALSTEP:
					return new Int32Property(ModelCode.RATIOTAPCHANGER_NOMINALSTEP, NominalStep);

				case ModelCode.RATIOTAPCHANGER_STEPCOUNT:
					return new Int32Property(ModelCode.RATIOTAPCHANGER_STEPCOUNT, StepCount);

				case ModelCode.RATIOTAPCHANGER_VOLTAGESTEP:
					return new FloatProperty(ModelCode.RATIOTAPCHANGER_VOLTAGESTEP, VoltageStep);

				case ModelCode.RATIOTAPCHANGER_TRANSFORMERWINDING:
					return new ReferenceProperty(ModelCode.RATIOTAPCHANGER_TRANSFORMERWINDING, TransformerWinding);
			}

			return base.GetProperty(p);
		}

		public override bool SetProperty(Property p)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.RATIOTAPCHANGER_NOMINALSTEP:
					NominalStep = ((Int32Property)p).Value;
					return true;

				case ModelCode.RATIOTAPCHANGER_STEPCOUNT:
					StepCount = ((Int32Property)p).Value;
					return true;

				case ModelCode.RATIOTAPCHANGER_VOLTAGESTEP:
					VoltageStep = ((FloatProperty)p).Value;
					return true;

				case ModelCode.RATIOTAPCHANGER_TRANSFORMERWINDING:
					TransformerWinding = ((ReferenceProperty)p).Value;
					return true;
			}

			return base.SetProperty(p);
		}

		public override void GetSourceReferences(Dictionary<ModelCode, long> dst)
		{
			dst[ModelCode.RATIOTAPCHANGER_TRANSFORMERWINDING] = TransformerWinding;
			base.GetSourceReferences(dst);
		}

		public override IdentifiedObject Clone()
		{
			return new RatioTapChanger(this);
		}
	}
}
