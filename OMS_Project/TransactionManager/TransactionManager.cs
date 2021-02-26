using Common.Transaction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TransactionManager
{
	class TransactionManager
	{
		class TimeoutThreadParam
		{
			public int TimeoutMilliseconds { get; private set; }
			public bool Terminate { get; set; }

			public TimeoutThreadParam(int timeoutMilliseconds)
			{
				TimeoutMilliseconds = timeoutMilliseconds;
				Terminate = true;
			}
		}

		const int enlistTimeoutMilliseconds = 60000;

		List<ITransaction> enlisted;
		readonly object l;
		bool inProgress;
		TimeoutThreadParam timeoutThreadParam;

		public TransactionManager()
		{
			enlisted = new List<ITransaction>();
			l = new object();
		}

		public bool StartEnlist()
		{
			lock(l)
			{
				if(inProgress)
					return false;

				inProgress = true;

				timeoutThreadParam = new TimeoutThreadParam(enlistTimeoutMilliseconds);
				new Thread(() => Timeout(timeoutThreadParam)).Start();
			}

			return true;
		}

		public bool Enlist(ITransaction client)
		{
			lock(l)
			{
				if(!inProgress)
					return false;

				lock(timeoutThreadParam)
				{
					enlisted.Add(client);
					timeoutThreadParam.Terminate = false;
				}

				timeoutThreadParam = new TimeoutThreadParam(enlistTimeoutMilliseconds);
				new Thread(() => Timeout(timeoutThreadParam)).Start();
			}

			return true;
		}

		public bool EndEnlist(bool ok)
		{
			lock(l)
			{
				if(!EndEnlistInternal(ok))
					return false;

				lock(timeoutThreadParam)
				{
					timeoutThreadParam.Terminate = false;
				}
			}

			return true;
		}

		bool EndEnlistInternal(bool ok)
		{
			if(!inProgress)
				return false;

			if(ok)
			{
				foreach(ITransaction client in enlisted) //prepare
				{
					try
					{
						ok = client.Prepare();
					}
					catch(Exception e)
					{
						ok = false;
					}

					if(!ok)
						break;
				}
			}

			if(ok)
			{
				foreach(ITransaction client in enlisted) //commit
				{
					try
					{
						client.Commit();
					}
					catch(Exception e)
					{ }
				}
			}
			else
			{
				foreach(ITransaction client in enlisted)  //rollback
				{
					try
					{
						client.Rollback();
					}
					catch(Exception e)
					{ }
				}
			}

			inProgress = false;
			enlisted.Clear();
			return true;
		}

		void Timeout(TimeoutThreadParam param)
		{
			Thread.Sleep(param.TimeoutMilliseconds);
			bool terminate;

			lock(l)
			{
				lock(param)
				{
					terminate = param.Terminate;
				}

				if(terminate)
					EndEnlistInternal(false);
			}
		}
	}
}
