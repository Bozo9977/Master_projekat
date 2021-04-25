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
		List<long> inserted, updated, deleted;

		public List<float> ReadAnalog(List<long> gids)
		{
			//return SimulatorProxy.Instance.GetAnalog(gids);
			return null;
		}

		public List<int> ReadDiscrete(List<long> gids)
		{
			//return SimulatorProxy.Instance.GetDiscrete(gids);
			return null;
		}

		public void CommandAnalog(List<long> gids, List<float> values)
		{
			//SimulatorProxy.Instance.CommandAnalog(gids, values);
		}

		public void CommandDiscrete(List<long> gids, List<int> values)
		{
			//SimulatorProxy.Instance.CommandDiscrete(gids, values);
		}

		public bool ApplyUpdate(List<long> inserted, List<long> updated, List<long> deleted)
		{
			bool ok;
			DuplexClient<ITransactionManager, ITransaction> client = new DuplexClient<ITransactionManager, ITransaction>("callbackEndpoint", this);
			client.Connect();

			if(!client.Call<bool>(tm => tm.Enlist(), out ok) || !ok)
			{
				client.Disconnect();
				return false;
			}

			FilterGIDs(inserted);
			FilterGIDs(updated);
			FilterGIDs(deleted);

			this.inserted = inserted;
			this.updated = updated;
			this.deleted = deleted;

			return true;
		}

		void FilterGIDs(List<long> gids)
		{
			int wi = 0;

			for(int ri = 0; ri < gids.Count; ++ri)
			{
				long gid = gids[ri];
				DMSType type = ModelCodeHelper.GetTypeFromGID(gid);

				if(type == DMSType.Analog || type == DMSType.Discrete)
				{
					gids[wi] = gid;
					++wi;
				}
			}

			gids.RemoveRange(wi, gids.Count - wi);
			gids.TrimExcess();
		}

		public bool Prepare()
		{
			SCADAModelDownload download = new SCADAModelDownload(inserted, updated, deleted);

			if(!download.Download())
			{
				return false;
			}

			SCADAModel tModel = new SCADAModel(SCADAModel.Instance);

			if(!tModel.ApplyUpdate(download))
			{
				return false;
			}

			Interlocked.Exchange(ref transactionModel, tModel);
			return tModel.PersistUpdate();
		}

		public void Commit()
		{
			SCADAModel.Instance = transactionModel;
			transactionModel.CommitUpdate();
			Interlocked.Exchange(ref transactionModel, null);
			FieldProxy.Instance.UpdateModel();
		}

		public void Rollback()
		{
			transactionModel.RollbackUpdate();
			Interlocked.Exchange(ref transactionModel, null);
		}
	}
}
