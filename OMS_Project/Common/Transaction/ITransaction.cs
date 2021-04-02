using System.ServiceModel;

namespace Common.Transaction
{
	public interface ITransaction
	{
		[OperationContract]
		bool Prepare();

		[OperationContract(IsOneWay = true)]
		void Commit();

		[OperationContract(IsOneWay = true)]
		void Rollback();
	}
}
