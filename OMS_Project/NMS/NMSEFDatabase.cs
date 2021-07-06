using System;
using System.Collections.Generic;
using Common.DataModel;
using Common.GDA;
using Common.EntityFramework;

namespace NMS
{
	public class NMSEFDatabase : INMSDatabase
	{
		IEFTable[] tables;

		public NMSEFDatabase()
		{
			tables = new IEFTable[] { new EFTable<ModelCounterDBModel>(), new EFTable<AnalogDBModel>(), new EFTable<DiscreteDBModel>(), new EFTable<ConnectivityNodeDBModel>(), new EFTable<TerminalDBModel>(), new EFTable<BaseVoltageDBModel>(), new EFTable<PowerTransformerDBModel>(), new EFTable<TransformerWindingDBModel>(), new EFTable<RatioTapChangerDBModel>(), new EFTable<EnergySourceDBModel>(), new EFTable<DistributionGeneratorDBModel>(), new EFTable<EnergyConsumerDBModel>(), new EFTable<ACLineSegmentDBModel>(), new EFTable<BreakerDBModel>(), new EFTable<RecloserDBModel>(), new EFTable<DisconnectorDBModel>() };
		}

		public List<IdentifiedObject> GetList(DMSType type)
		{
			List<IdentifiedObject> l = new List<IdentifiedObject>();

			try
			{
				using(DBContext context = new DBContext())
				{
					foreach(object entity in tables[(int)type].GetList(context))
					{
						l.Add(IdentifiedObject.Load(type, entity));
					}
				}
			}
			catch(Exception e)
			{
				return null;
			}

			return l;
		}

		public Dictionary<DMSType, int> GetCounters()
		{
			Dictionary<DMSType, int> d = new Dictionary<DMSType, int>();

			try
			{
				using(DBContext context = new DBContext())
				{
					foreach(ModelCounterDBModel counter in tables[0].GetList(context))
					{
						d.Add(counter.Type, counter.Counter);
					}
				}
			}
			catch(Exception e)
			{
				return null;
			}

			return d;
		}

		public bool PersistDelta(List<IdentifiedObject> inserted, List<IdentifiedObject> updatedNew, List<IdentifiedObject> deleted, Dictionary<DMSType, int> newCounters)
		{
			try
			{
				using(DBContext context = new DBContext())
				{
					for(int i = 0; i < inserted.Count; ++i)
					{
						IdentifiedObject io = inserted[i];
						IEFTable table = tables[(int)ModelCodeHelper.GetTypeFromGID(io.GID)];
						object entity = io.ToDBEntity();
						table.Insert(context, entity);
					}

					for(int i = 0; i < updatedNew.Count; ++i)
					{
						IdentifiedObject io = updatedNew[i];
						IEFTable table = tables[(int)ModelCodeHelper.GetTypeFromGID(io.GID)];
						object entity = io.ToDBEntity();
						table.Update(context, entity);
					}

					for(int i = 0; i < deleted.Count; ++i)
					{
						IdentifiedObject io = deleted[i];
						IEFTable table = tables[(int)ModelCodeHelper.GetTypeFromGID(io.GID)];
						object entity = io.ToDBEntity();
						table.Delete(context, entity);
					}

					foreach(KeyValuePair<DMSType, int> pair in newCounters)
					{
						IEFTable table = tables[0];
						ModelCounterDBModel oldCounter = (ModelCounterDBModel)table.Get(context, pair.Key);
						
						if(oldCounter != null)
						{
							oldCounter.Counter = pair.Value;
						}
						else
						{
							table.Insert(context, new ModelCounterDBModel() { Type = pair.Key, Counter = pair.Value });
						}
					}

					context.SaveChanges();
				}
			}
			catch(Exception e)
			{
				return false;
			}

			return true;
		}

		public bool RollbackDelta(List<IdentifiedObject> inserted, List<IdentifiedObject> updatedOld, List<IdentifiedObject> deleted, Dictionary<DMSType, int> oldCounters)
		{
			try
			{
				using(DBContext context = new DBContext())
				{
					for(int i = deleted.Count - 1; i >= 0; --i)
					{
						IdentifiedObject io = deleted[i];
						IEFTable table = tables[(int)ModelCodeHelper.GetTypeFromGID(io.GID)];
						object entity = io.ToDBEntity();
						table.Insert(context, entity);
					}

					for(int i = updatedOld.Count - 1; i >= 0; --i)
					{
						IdentifiedObject io = updatedOld[i];
						IEFTable table = tables[(int)ModelCodeHelper.GetTypeFromGID(io.GID)];
						object entity = io.ToDBEntity();
						table.Update(context, entity);
					}

					for(int i = inserted.Count - 1; i >= 0; --i)
					{
						IdentifiedObject io = inserted[i];
						IEFTable table = tables[(int)ModelCodeHelper.GetTypeFromGID(io.GID)];
						object entity = io.ToDBEntity();
						table.Delete(context, entity);
					}

					foreach(KeyValuePair<DMSType, int> pair in oldCounters)
					{
						IEFTable table = tables[0];
						ModelCounterDBModel oldCounter = (ModelCounterDBModel)table.Get(context, pair.Key);
						
						if(oldCounter != null)
						{
							oldCounter.Counter = pair.Value;
						}
						else
						{
							table.Insert(context, new ModelCounterDBModel() { Type = pair.Key, Counter = pair.Value });
						}
					}

					context.SaveChanges();
				}
			}
			catch(Exception e)
			{
				return false;
			}

			return true;
		}
	}
}
