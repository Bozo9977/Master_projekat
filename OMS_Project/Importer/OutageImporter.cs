using CIM.Model;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importer
{
	class OutageImporter
	{
		Dictionary<DMSType, int> counters;
		Dictionary<string, long> idMapping;

		public OutageImporter()
		{
			Array types = Enum.GetValues(typeof(DMSType));
			counters = new Dictionary<DMSType, int>(types.Length);

			foreach(DMSType t in types)
				counters.Add(t, 0);

			idMapping = new Dictionary<string, long>();
		}

		public Delta CreateNMSDelta(ConcreteModel concreteModel)
		{
			if(concreteModel == null || concreteModel.ModelMap == null)
				return null;

			Delta delta = new Delta();

			//import all concrete model types (DMSType enum)
			ImportBaseVoltages(concreteModel, delta);
			//ImportLocations(delta);
			//ImportPowerTransformers(delta);
			//ImportTransformerWindings(delta);
			//ImportWindingTests(delta);

			return delta;
		}

		private bool ImportBaseVoltages(ConcreteModel concreteModel, Delta delta)
		{
			SortedDictionary<string, object> cimBaseVoltages = concreteModel.GetAllObjectsOfType("FTN.BaseVoltage");

			if(cimBaseVoltages == null)
				return false;

			foreach(KeyValuePair<string, object> cimBaseVoltagePair in cimBaseVoltages)
			{
				FTN.BaseVoltage cimBaseVoltage = cimBaseVoltagePair.Value as FTN.BaseVoltage;
				ResourceDescription rd = CreateBaseVoltageResourceDescription(cimBaseVoltage);

				if(rd == null)
					return false;

				delta.InsertOperations.Add(rd);
			}

			return true;
		}

		private ResourceDescription CreateBaseVoltageResourceDescription(FTN.BaseVoltage cimBaseVoltage)
		{
			if(cimBaseVoltage == null)
				return null;

			long gid = ModelCodeHelper.CreateGID(0, DMSType.BASEVOLTAGE, ++counters[DMSType.BASEVOLTAGE]);
			ResourceDescription rd = new ResourceDescription(gid);
			idMapping[cimBaseVoltage.ID] = gid;
			OutageConverter.PopulateBaseVoltageProperties(cimBaseVoltage, rd);
			return rd;
		}

		//...
	}
}
