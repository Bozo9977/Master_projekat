using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Transaction;

namespace TransactionManager
{
	class TransactionManagerService : ITransactionManager
	{
		public bool StartEnlist()
		{
			throw new NotImplementedException();
		}

		public bool Enlist()
		{
			throw new NotImplementedException();
		}

		public bool EndEnlist(bool ok)
		{
			throw new NotImplementedException();
		}
	}
}
