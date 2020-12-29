using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.GDA
{
	[ServiceContract]
	public interface INetworkModelGDAContract
	{
		[OperationContract]
		UpdateResult ApplyUpdate(Delta delta);

		[OperationContract]
		ResourceDescription GetValues(long resourceId, List<ModelCode> propIds);

		[OperationContract]
		int GetExtentValues(ModelCode entityType, List<ModelCode> propIds);

		[OperationContract]
		int GetRelatedValues(long source, List<ModelCode> propIds, Association association);

		[OperationContract]
		List<ResourceDescription> IteratorNext(int n, int id);

		[OperationContract]
		bool IteratorRewind(int id);

		[OperationContract]
		int IteratorResourcesTotal(int id);

		[OperationContract]
		int IteratorResourcesLeft(int id);

		[OperationContract]
		bool IteratorClose(int id);
	}
}
