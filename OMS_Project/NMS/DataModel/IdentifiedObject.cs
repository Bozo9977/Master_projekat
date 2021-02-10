using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS.DataModel
{
	abstract class IdentifiedObject
	{
		public long GID { get; private set; }
		public string Name { get; private set; }
		public string MRID { get; private set; }

		public IdentifiedObject() { }

		public IdentifiedObject(IdentifiedObject io)
		{
			GID = io.GID;
			Name = io.Name;
			MRID = io.MRID;
		}

		public static IdentifiedObject Create(ResourceDescription rd)
		{
			IdentifiedObject io = null;

			switch(ModelCodeHelper.GetTypeFromGID(rd.Id))
			{
				case DMSType.ACLineSegment:
					io = new ACLineSegment();
					break;
				case DMSType.Analog:
					io = new Analog();
					break;
				case DMSType.BaseVoltage:
					io = new BaseVoltage();
					break;
				case DMSType.Breaker:
					io = new Breaker();
					break;
				case DMSType.ConnectivityNode:
					io = new ConnectivityNode();
					break;
				case DMSType.Disconnector:
					io = new Disconnector();
					break;
				case DMSType.Discrete:
					io = new Discrete();
					break;
				case DMSType.DistributionGenerator:
					io = new DistributionGenerator();
					break;
				case DMSType.EnergyConsumer:
					io = new EnergyConsumer();
					break;
				case DMSType.EnergySource:
					io = new EnergySource();
					break;
				case DMSType.PowerTransformer:
					io = new PowerTransformer();
					break;
				case DMSType.RatioTapChanger:
					io = new RatioTapChanger();
					break;
				case DMSType.Recloser:
					io = new Recloser();
					break;
				case DMSType.Terminal:
					io = new Terminal();
					break;
				case DMSType.TransformerWinding:
					io = new TransformerWinding();
					break;

				default:
					return null;
			}

			io.GID = rd.Id;

			foreach(Property p in rd.Properties.Values)
				if(!io.SetProperty(p))
					return null;

			return io;
		}

		public virtual bool HasProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.IDENTIFIEDOBJECT_GID:
				case ModelCode.IDENTIFIEDOBJECT_NAME:
				case ModelCode.IDENTIFIEDOBJECT_MRID:
					return true;
			}

			return false;
		}

		public virtual Property GetProperty(ModelCode p)
		{
			switch(p)
			{
				case ModelCode.IDENTIFIEDOBJECT_GID:
					return new Int64Property(ModelCode.IDENTIFIEDOBJECT_GID, GID);

				case ModelCode.IDENTIFIEDOBJECT_NAME:
					return new StringProperty(ModelCode.IDENTIFIEDOBJECT_NAME, Name);

				case ModelCode.IDENTIFIEDOBJECT_MRID:
					return new StringProperty(ModelCode.IDENTIFIEDOBJECT_MRID, MRID);
			}

			return null;
		}

		public virtual bool SetProperty(Property p)
		{
			if(p == null)
				return false;

			switch(p.Id)
			{
				case ModelCode.IDENTIFIEDOBJECT_NAME:
					Name = ((StringProperty)p).Value;
					return true;

				case ModelCode.IDENTIFIEDOBJECT_MRID:
					MRID = ((StringProperty)p).Value;
					return true;
			}

			return false;
		}

		public virtual bool AddTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			return false;
		}

		public virtual bool RemoveTargetReference(ModelCode sourceProperty, long sourceGID)
		{
			return false;
		}

		public virtual void GetTargetReferences(Dictionary<ModelCode, List<long>> dst)
		{ }

		public virtual void GetSourceReferences(Dictionary<ModelCode, long> dst)
		{ }

		public virtual bool IsReferenced()
		{
			return false;
		}

		public virtual bool Validate()
		{
			return true;
		}

		public virtual void GetEntitiesToValidate(Func<long, IdentifiedObject> entityGetter, HashSet<long> dst)
		{ }

		public abstract IdentifiedObject Clone();
	}
}
