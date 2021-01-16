using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Importer
{
	class OutageConverter
	{
		public static void PopulateIdentifiedObjectProperties(FTN.IdentifiedObject cimIdentifiedObject, ResourceDescription rd)
		{
			if(cimIdentifiedObject == null || rd == null)
				return;

			if(cimIdentifiedObject.MRIDHasValue)
				rd.AddProperty(new StringProperty(ModelCode.IDENTIFIEDOBJECT_MRID, cimIdentifiedObject.MRID));

			if(cimIdentifiedObject.NameHasValue)
				rd.AddProperty(new StringProperty(ModelCode.IDENTIFIEDOBJECT_NAME, cimIdentifiedObject.Name));
		}

		public static void PopulateBaseVoltageProperties(FTN.BaseVoltage cimBaseVoltage, ResourceDescription rd)
		{
			if((cimBaseVoltage != null) && (rd != null))
			{
				OutageConverter.PopulateIdentifiedObjectProperties(cimBaseVoltage, rd);

				if(cimBaseVoltage.NominalVoltageHasValue)
					rd.AddProperty(new FloatProperty(ModelCode.BASEVOLTAGE_NOMINALVOLTAGE, cimBaseVoltage.NominalVoltage));
			}
		}

		//...
	}
}
