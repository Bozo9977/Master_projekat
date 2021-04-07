using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI
{
	class GraphicsModelMapping
	{
		static object instanceLock = new object();
		static GraphicsModelMapping instance;
		Dictionary<DMSType, object> typeToGE;

		public static GraphicsModelMapping Instance
		{
			get
			{
				if(instance == null)
				{
					lock(instanceLock)
					{
						if(instance == null)
						{
							instance = new GraphicsModelMapping();
						}
					}
				}

				return instance;
			}
		}

		GraphicsModelMapping()
		{
			typeToGE = new Dictionary<DMSType, object>();

			typeToGE[DMSType.ACLineSegment] = new GraphicsModels.ACLineSegment();
			typeToGE[DMSType.Breaker] = new GraphicsModels.Breaker();
			typeToGE[DMSType.ConnectivityNode] = new GraphicsModels.ConnectivityNode();
			typeToGE[DMSType.Disconnector] = new GraphicsModels.Disconnector();
			typeToGE[DMSType.DistributionGenerator] = new GraphicsModels.DistributionGenerator();
			typeToGE[DMSType.EnergyConsumer] = new Dictionary<ConsumerClass, GraphicsModel> { { ConsumerClass.Administrative, new GraphicsModels.AdministrativeConsumer() }, { ConsumerClass.Residential, new GraphicsModels.ResidentialConsumer() }, { ConsumerClass.Industrial, new GraphicsModels.IndustrialConsumer() } };
			typeToGE[DMSType.EnergySource] = new GraphicsModels.EnergySource();
			typeToGE[DMSType.Recloser] = new GraphicsModels.Recloser();
			typeToGE[DMSType.TransformerWinding] = new GraphicsModels.TransformerWinding();
		}

		public GraphicsModel GetGraphicsModel(IdentifiedObject io)
		{
			DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);
			object model;

			if(!typeToGE.TryGetValue(type, out model))
				return null;

			if(type == DMSType.EnergyConsumer)
			{
				model = ((Dictionary<ConsumerClass, GraphicsModel>)model)[((EnergyConsumer)io).ConsumerClass];
			}

			return (GraphicsModel)model;
		}
	}
}
