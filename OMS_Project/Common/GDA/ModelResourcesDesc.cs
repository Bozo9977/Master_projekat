using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.GDA
{
	public class ModelResourcesDesc
	{
		List<ModelCode> typeIdsInInsertOrder = new List<ModelCode>();

		public List<ModelCode> TypeIdsInInsertOrder
		{
			get
			{
				return typeIdsInInsertOrder;
			}
		}

		private void InitializeTypeIdsInInsertOrder()
		{
			/*typeIdsInInsertOrder.Add(ModelCode.BASEVOLTAGE);
			typeIdsInInsertOrder.Add(ModelCode.LOCATION);
			typeIdsInInsertOrder.Add(ModelCode.POWERTR);
			typeIdsInInsertOrder.Add(ModelCode.POWERTRWINDING);
			typeIdsInInsertOrder.Add(ModelCode.WINDINGTEST);*/
		}
	}
}
