using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

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
