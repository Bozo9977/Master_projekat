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
	public class RatioTapChangerDBModel
	{
		[Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
		public long GID { get; set; }
		public string MRID { get; set; }
		public string Name { get; set; }
		public int NominalStep { get; set; }
		public int StepCount { get; set; }
		public float VoltageStep { get; set; }
		public long TransformerWinding { get; set; }
	}

	public class RatioTapChanger : TapChanger
	{
		public int NominalStep { get; protected set; }
		public int StepCount { get; protected set; }
		public float VoltageStep { get; protected set; }
		public long TransformerWinding { get; protected set; }

		public RatioTapChanger() { }

		public RatioTapChanger(RatioTapChanger r) : base(r)
		{
			NominalStep = r.NominalStep;
			StepCount = r.StepCount;
			VoltageStep = r.VoltageStep;
			TransformerWinding = r.TransformerWinding;
		}

		public RatioTapChanger(RatioTapChangerDBModel entity)
		{
			GID = entity.GID;
			MRID = entity.MRID;
			Name = entity.Name;
			NominalStep = entity.NominalStep;
			StepCount = entity.StepCount;
			VoltageStep = entity.VoltageStep;
			TransformerWinding = entity.TransformerWinding;
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

		public override bool SetProperty(Property p, bool force = false)
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

			return base.SetProperty(p, force);
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

		public override object ToDBEntity()
		{
			return new RatioTapChangerDBModel() { GID = GID, MRID = MRID, Name = Name, StepCount = StepCount, NominalStep = NominalStep, VoltageStep = VoltageStep = TransformerWinding = TransformerWinding };
		}

		public override bool Validate(Func<long, IdentifiedObject> entityGetter)
		{
			if(StepCount < 0 || NominalStep < 0 || VoltageStep < 0)
				return false;

			return base.Validate(entityGetter);
		}
	}
}
