using System.ServiceModel;

namespace Common.Transaction
{
	[ServiceContract(CallbackContract = typeof(ITransaction))]
	public interface ITransactionManager
	{
		[OperationContract]
		bool StartEnlist();

		[OperationContract]
		bool Enlist();

		[OperationContract]
		bool EndEnlist(bool ok);
	}
}
