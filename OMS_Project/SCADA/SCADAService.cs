using Common.GDA;
using Common.SCADA;
using Common.Transaction;
using Common.WCF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SCADA
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	class SCADAService : ISCADAServiceContract, ITransaction
	{
		SCADAModel transactionModel;

		public List<float> ReadAnalog(List<long> gids)
		{
			return SCADAModel.Instance.GetAnalog(gids);
		}

		public List<int> ReadDiscrete(List<long> gids)
		{
			return SCADAModel.Instance.GetDiscrete(gids);
		}

		public void CommandAnalog(List<long> gids, List<float> values)
		{
			//SimulatorProxy.Instance.CommandAnalog(gids, values);
		}

		public void CommandDiscrete(List<long> gids, List<int> values)
		{
			//SimulatorProxy.Instance.CommandDiscrete(gids, values);
		}

		public bool ApplyUpdate()
		{
			bool ok;
			DuplexClient<ITransactionManager, ITransaction> client = new DuplexClient<ITransactionManager, ITransaction>("callbackEndpoint", this);
			client.Connect();

			if(!client.Call<bool>(tm => tm.Enlist(), out ok) || !ok)
			{
				client.Disconnect();
				return false;
			}

			return true;
		}

		public bool Prepare()
		{
			SCADAModelDownload download = new SCADAModelDownload();

			if(!download.Download())
			{
				return false;
			}

			SCADAModel tModel = new SCADAModel(download);
			Interlocked.Exchange(ref transactionModel, tModel);
			return true;
		}

		public void Commit()
		{
			SCADAModel.Instance = transactionModel;
		}

		public void Rollback()
		{
			
		}
	}
}
