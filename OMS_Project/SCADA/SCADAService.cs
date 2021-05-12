using Common.GDA;
using Common.SCADA;
using Common.Transaction;
using Common.WCF;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace SCADA
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	class SCADAService : ISCADAServiceContract, ITransaction
	{
		static object updateLock = new object();
		static SCADAModel transactionModel;
		static List<long> inserted, updated, deleted;

		public List<Tuple<long, float>> ReadAnalog(List<long> gids)
		{
			if(gids == null)
				return null;

			return FieldProxy.Instance.ReadAnalog(gids);
		}

		public List<Tuple<long, int>> ReadDiscrete(List<long> gids)
		{
			if(gids == null)
				return null;

			return FieldProxy.Instance.ReadDiscrete(gids);
		}

		public void CommandAnalog(List<long> gids, List<float> values)
		{
			if(gids == null || values == null)
				return;

			FieldProxy.Instance.CommandAnalog(gids, values);
		}

		public void CommandDiscrete(List<long> gids, List<int> values)
		{
			if(gids == null || values == null)
				return;

			FieldProxy.Instance.CommandDiscrete(gids, values);
		}

		public bool ApplyUpdate(List<long> inserted, List<long> updated, List<long> deleted)
		{
			if(inserted == null || updated == null || deleted == null)
				return false;

			lock(updateLock)
			{
				if(SCADAService.inserted != null)
					return false;

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

				SCADAService.inserted = inserted;
				SCADAService.updated = updated;
				SCADAService.deleted = deleted;

				return true;
			}
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
			lock(updateLock)
			{
				if(inserted == null)
					return false;

				SCADAModelDownload download = new SCADAModelDownload(inserted, updated, deleted);

				if(!download.Download())
					return false;

				SCADAModel tModel = new SCADAModel(SCADAModel.Instance);

				if(!tModel.ApplyUpdate(download))
					return false;

				if(!tModel.PersistUpdate())
					return false;

				transactionModel = tModel;
				inserted = null;
				updated = null;
				deleted = null;

				return true;
			}
		}

		public void Commit()
		{
			lock(updateLock)
			{
				if(transactionModel == null)
					return;

				transactionModel.CommitUpdate();
				SCADAModel.Instance = transactionModel;
				FieldProxy.Instance.UpdateModel();

				transactionModel = null;
			}
		}

		public void Rollback()
		{
			lock(updateLock)
			{
				if(transactionModel != null)
				{
					transactionModel.RollbackUpdate();
					transactionModel = null;
				}

				inserted = null;
				updated = null;
				deleted = null;
			}
		}
	}
}
