using Common.Database;
using Common.DataModel;
using Common.GDA;
using System;
using System.Collections.Generic;

namespace SCADA
{
	public class SCADAModel
	{
		static volatile SCADAModel instance;

		Dictionary<long, Analog> analogs;
		Dictionary<long, Discrete> discretes;
		IDatabase<ESCADAModelDatabaseTables> db;

		List<IdentifiedObject> inserted;
		List<IdentifiedObject> updatedOld;
		List<IdentifiedObject> updatedNew;
		List<IdentifiedObject> deleted;

		public static SCADAModel Instance
		{
			get
			{
				return instance;
			}
			set
			{
				instance = value;
			}
		}

		public SCADAModel()
		{
			analogs = new Dictionary<long, Analog>();
			discretes = new Dictionary<long, Discrete>();
		}

		public SCADAModel(SCADAModel model)
		{
			analogs = new Dictionary<long, Analog>(model.analogs);
			discretes = new Dictionary<long, Discrete>(model.discretes);
			db = model.db;
		}

		public List<long> GetDiscreteGIDs()
		{
			return new List<long>(discretes.Keys);
		}

		public List<long> GetAnalogGIDs()
		{
			return new List<long>(analogs.Keys);
		}

		public SCADAModel(IDatabase<ESCADAModelDatabaseTables> db)
		{
			analogs = new Dictionary<long, Analog>();
			discretes = new Dictionary<long, Discrete>();

			if(!db.Transact(transaction =>
			{
				ITableContext analogs = transaction.GetTable(ESCADAModelDatabaseTables.Analogs);
				ITableContext discretes = transaction.GetTable(ESCADAModelDatabaseTables.Discretes);

				foreach(AnalogDBModel a in analogs.GetList())
				{
					Analog analog = (Analog)IdentifiedObject.Load(DMSType.Analog, a);
					this.analogs.Add(analog.GID, analog);
				}

				foreach(DiscreteDBModel d in discretes.GetList())
				{
					Discrete discrete = (Discrete)IdentifiedObject.Load(DMSType.Discrete, d);
					this.discretes.Add(discrete.GID, discrete);
				}

				return true;
			}))
			{
				throw new Exception("Cannot load SCADA model from database.");
			}

			this.db = db;
		}

		public Analog GetAnalog(long gid)
		{
			Analog a;
			if(!analogs.TryGetValue(gid, out a))
				return null;

			return new Analog(a);
		}

		public Discrete GetDiscrete(long gid)
		{
			Discrete d;
			if(!discretes.TryGetValue(gid, out d))
				return null;

			return new Discrete(d);
		}

		public bool ApplyUpdate(SCADAModelDownload download)
		{
			List<IdentifiedObject> inserted = new List<IdentifiedObject>(download.Inserted.Count);

			foreach(IdentifiedObject io in download.Inserted)
			{
				DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);

				if(type == DMSType.Analog)
				{
					Analog analog = (Analog)io;
					int oldCount = analogs.Count;
					analogs[io.GID] = analog;

					if(oldCount == analogs.Count)
						return false;
				}
				else if(type == DMSType.Discrete)
				{
					Discrete discrete = (Discrete)io;
					int oldCount = discretes.Count;
					discretes[io.GID] = discrete;

					if(oldCount == discretes.Count)
						return false;
				}
				else
				{
					return false;
				}

				inserted.Add(io);
			}

			List<IdentifiedObject> updatedOld = new List<IdentifiedObject>(download.Updated.Count);
			List<IdentifiedObject> updatedNew = new List<IdentifiedObject>(download.Updated.Count);

			foreach(IdentifiedObject io in download.Updated)
			{
				DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);

				if(type == DMSType.Analog)
				{
					Analog oldAnalog;

					if(!analogs.TryGetValue(io.GID, out oldAnalog))
						return false;

					Analog newAnalog = (Analog)io;
					analogs[io.GID] = newAnalog;

					updatedOld.Add(oldAnalog);
				}
				else if(type == DMSType.Discrete)
				{
					Discrete oldDiscrete;

					if(!discretes.TryGetValue(io.GID, out oldDiscrete))
						return false;

					Discrete newDiscrete = (Discrete)io;
					discretes[io.GID] = newDiscrete;

					updatedOld.Add(oldDiscrete);
				}
				else
				{
					return false;
				}
				
				updatedNew.Add(io);
			}

			List<IdentifiedObject> deleted = new List<IdentifiedObject>(download.Deleted.Count);

			foreach(long gid in download.Deleted)
			{
				DMSType type = ModelCodeHelper.GetTypeFromGID(gid);

				if(type == DMSType.Analog)
				{
					Analog analog;

					if(!analogs.TryGetValue(gid, out analog) || !analogs.Remove(gid))
						return false;

					deleted.Add(analog);
				}
				else if(type == DMSType.Discrete)
				{
					Discrete discrete;

					if(!discretes.TryGetValue(gid, out discrete) || !discretes.Remove(gid))
						return false;

					deleted.Add(discrete);
				}
				else
				{
					return false;
				}
			}

			this.inserted = inserted;
			this.updatedOld = updatedOld;
			this.updatedNew = updatedNew;
			this.deleted = deleted;

			return true;
		}

		public bool PersistUpdate()
		{
			return db != null && inserted != null && db.Transact(transaction =>
			{
				ITableContext analogs = transaction.GetTable(ESCADAModelDatabaseTables.Analogs);
				ITableContext discretes = transaction.GetTable(ESCADAModelDatabaseTables.Discretes);

				foreach(IdentifiedObject io in inserted)
				{
					DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);

					if(type == DMSType.Analog)
					{
						analogs.Insert(io.ToDBEntity());
					}
					else
					{
						discretes.Insert(io.ToDBEntity());
					}
				}

				foreach(IdentifiedObject io in updatedNew)
				{
					DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);

					if(type == DMSType.Analog)
					{
						analogs.Update(io.ToDBEntity());
					}
					else
					{
						discretes.Update(io.ToDBEntity());
					}
				}

				foreach(IdentifiedObject io in deleted)
				{
					DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);

					if(type == DMSType.Analog)
					{
						analogs.Delete(io.ToDBEntity());
					}
					else
					{
						discretes.Delete(io.ToDBEntity());
					}
				}

				return true;
			});
		}

		public bool CommitUpdate()
		{
			inserted = null;
			updatedNew = null;
			updatedOld = null;
			deleted = null;
			return true;
		}

		public bool RollbackUpdate()
		{
			return db != null && inserted != null && db.Transact(transaction =>
			{
				ITableContext analogs = transaction.GetTable(ESCADAModelDatabaseTables.Analogs);
				ITableContext discretes = transaction.GetTable(ESCADAModelDatabaseTables.Discretes);

				for(int i = deleted.Count - 1; i >= 0; --i)
				{
					IdentifiedObject io = deleted[i];
					DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);

					if(type == DMSType.Analog)
					{
						analogs.Insert(io.ToDBEntity());
					}
					else
					{
						discretes.Insert(io.ToDBEntity());
					}
				}

				for(int i = updatedOld.Count - 1; i >= 0; --i)
				{
					IdentifiedObject io = updatedOld[i];
					DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);

					if(type == DMSType.Analog)
					{
						analogs.Update(io.ToDBEntity());
					}
					else
					{
						discretes.Update(io.ToDBEntity());
					}
				}

				for(int i = inserted.Count - 1; i >= 0; --i)
				{
					IdentifiedObject io = inserted[i];
					DMSType type = ModelCodeHelper.GetTypeFromGID(io.GID);

					if(type == DMSType.Analog)
					{
						analogs.Delete(io.ToDBEntity());
					}
					else
					{
						discretes.Delete(io.ToDBEntity());
					}
				}

				return true;
			});
		}
	}
}
