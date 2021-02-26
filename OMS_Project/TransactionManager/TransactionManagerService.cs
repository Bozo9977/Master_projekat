using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.ServiceModel;
using System.ServiceModel.Channels;
using System.Text;
using System.Threading.Tasks;
using Common.Transaction;

namespace TransactionManager
{
	[ServiceBehavior(ConcurrencyMode = ConcurrencyMode.Reentrant)]
	class TransactionManagerService : ITransactionManager
	{
		static TransactionManager transactionManager = new TransactionManager();

		public bool StartEnlist()
		{
			return transactionManager.StartEnlist();
		}

		public bool Enlist()
		{
			ITransaction client = OperationContext.Current.GetCallbackChannel<ITransaction>();
			return transactionManager.Enlist(client);
		}

		public bool EndEnlist(bool ok)
		{
			return transactionManager.EndEnlist(ok);
		}
	}
}
