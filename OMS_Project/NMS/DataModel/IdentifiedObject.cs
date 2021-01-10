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
				case DMSType.ACLINESEGMENT:
					io = new ACLineSegment();
					break;
				case DMSType.ANALOG:
					io = new Analog();
					break;
				case DMSType.BASEVOLTAGE:
					io = new BaseVoltage();
					break;
				case DMSType.BREAKER:
					io = new Breaker();
					break;
				case DMSType.CONNECTIVITYNODE:
					io = new ConnectivityNode();
					break;
				case DMSType.DISCONNECTOR:
					io = new Disconnector();
					break;
				case DMSType.DISCRETE:
					io = new Discrete();
					break;
				case DMSType.DISTRIBUTIONGENERATOR:
					io = new DistributionGenerator();
					break;
				case DMSType.ENERGYCONSUMER:
					io = new EnergyConsumer();
					break;
				case DMSType.ENERGYSOURCE:
					io = new EnergySource();
					break;
				case DMSType.POWERTRANSFORMER:
					io = new PowerTransformer();
					break;
				case DMSType.RATIOTAPCHANGER:
					io = new RatioTapChanger();
					break;
				case DMSType.RECLOSER:
					io = new Recloser();
					break;
				case DMSType.TERMINAL:
					io = new Terminal();
					break;
				case DMSType.TRANSFORMERWINDING:
					io = new TransformerWinding();
					break;

				default:
					return null;
			}

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
				case ModelCode.IDENTIFIEDOBJECT_GID:
					GID = ((Int64Property)p).Value;
					return true;

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

		public abstract IdentifiedObject Clone();
	}
}
