using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Common.Transaction
{
	[ServiceContract]
	public interface ITransactionManager
	{
		[OperationContract]
		bool StartEnlist();

		[OperationContract]
		void Enlist();

		[OperationContract]
		bool EndEnlist(bool ok);
	}
}
