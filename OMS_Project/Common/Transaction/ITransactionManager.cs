using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
