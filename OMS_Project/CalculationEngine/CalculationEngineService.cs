using Common.CalculationEngine;
using Common.PubSub;
using Common.Transaction;
using Common.WCF;
using System;
using System.Collections.Generic;
using System.ServiceModel;

namespace CalculationEngine
{
	[CallbackBehavior(ConcurrencyMode = ConcurrencyMode.Multiple)]
	class CalculationEngineService : ICalculationEngineServiceContract, ITransaction
	{
		static object updateLock = new object();
		static TopologyModel transactionModel;
		static List<long> inserted, updated, deleted;

		public bool ApplyUpdate(List<long> inserted, List<long> updated, List<long> deleted)
		{
			if(inserted == null || updated == null || deleted == null)
				return false;

			lock(updateLock)
			{
				if(CalculationEngineService.inserted != null)
					return false;

				bool ok;
				DuplexClient<ITransactionManager, ITransaction> client = new DuplexClient<ITransactionManager, ITransaction>("callbackEndpoint", this);
				client.Connect();

				if(!client.Call<bool>(tm => tm.Enlist(), out ok) || !ok)
				{
					client.Disconnect();
					return false;
				}

				CalculationEngineService.inserted = inserted;
				CalculationEngineService.updated = updated;
				CalculationEngineService.deleted = deleted;

				return true;
			}
		}

		public bool Prepare()
		{
			lock(updateLock)
			{
				if(inserted == null)
					return false;

				TopologyModelDownload download = new TopologyModelDownload(inserted, updated, deleted);

				if(!download.Download())
					return false;

				TopologyModel tModel = new TopologyModel(TopologyModel.Instance);

				if(!tModel.ApplyUpdate(download))
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

				TopologyModel.Instance = transactionModel;
				Measurements.Instance.UpdateModel();
				transactionModel = null;

				Client<IPublishing> pubClient = new Client<IPublishing>("publishingEndpoint");
				pubClient.Connect();

				pubClient.Call<bool>(pub =>
				{
					pub.Publish(new TopologyChanged());
					return true;
				}, out _);

				pubClient.Disconnect();
			}
		}

		public void Rollback()
		{
			lock(updateLock)
			{
				transactionModel = null;

				inserted = null;
				updated = null;
				deleted = null;
			}
		}

		public List<Tuple<long, List<Tuple<long, long>>, List<Tuple<long, long>>>> GetLineEnergization()
		{
			return TopologyModel.Instance.GetLineEnergization();
		}

		public void UpdateMeasurements(List<Tuple<long, float>> analogInputs, List<Tuple<long, int>> discreteInputs)
		{
			Measurements measurements = Measurements.Instance;
			measurements.SetAnalogs(analogInputs);
			measurements.SetDiscretes(discreteInputs);
			TopologyModel.Instance.MeasurementsUpdated(analogInputs, discreteInputs);
		}
	}
}
