using System;
using System.Collections.Generic;
using Common.DataModel;
using Common.GDA;

namespace NMS
{
	public class NMSEFDatabase : INMSDatabase
	{
		IEFTable[] tables;

		public NMSEFDatabase()
		{
			tables = new IEFTable[] { new EFTable<AnalogDBModel>(), new EFTable<DiscreteDBModel>(), new EFTable<ConnectivityNodeDBModel>(), new EFTable<TerminalDBModel>(), new EFTable<BaseVoltageDBModel>(), new EFTable<PowerTransformerDBModel>(), new EFTable<TransformerWindingDBModel>(), new EFTable<RatioTapChangerDBModel>(), new EFTable<EnergySourceDBModel>(), new EFTable<DistributionGeneratorDBModel>(), new EFTable<EnergyConsumerDBModel>(), new EFTable<ACLineSegmentDBModel>(), new EFTable<BreakerDBModel>(), new EFTable<RecloserDBModel>(), new EFTable<DisconnectorDBModel>() };
		}

		public List<IdentifiedObject> GetList(DMSType type)
		{
			List<IdentifiedObject> l = new List<IdentifiedObject>();

			try
			{
				using(DBContext context = new DBContext())
				{
					foreach(object entity in tables[(int)type - 1].GetList(context))
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

		public bool PersistDelta(List<IdentifiedObject> inserted, List<IdentifiedObject> updatedNew, List<IdentifiedObject> deleted)
		{
			try
			{
				using(DBContext context = new DBContext())
				{
					foreach(IdentifiedObject io in inserted)
					{
						IEFTable table = tables[(int)ModelCodeHelper.GetTypeFromGID(io.GID) - 1];
						object entity = io.ToDBEntity();
						table.Insert(context, entity);
					}

					foreach(IdentifiedObject io in updatedNew)
					{
						IEFTable table = tables[(int)ModelCodeHelper.GetTypeFromGID(io.GID) - 1];
						object entity = io.ToDBEntity();
						table.Update(context, entity);
					}

					foreach(IdentifiedObject io in deleted)
					{
						IEFTable table = tables[(int)ModelCodeHelper.GetTypeFromGID(io.GID) - 1];
						object entity = io.ToDBEntity();
						table.Delete(context, entity);
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

		public bool RollbackDelta(List<IdentifiedObject> inserted, List<IdentifiedObject> updatedOld, List<IdentifiedObject> deleted)
		{
			try
			{
				using(DBContext context = new DBContext())
				{
					for(int i = deleted.Count - 1; i >= 0; --i)
					{
						IdentifiedObject io = deleted[i];
						IEFTable table = tables[(int)ModelCodeHelper.GetTypeFromGID(io.GID) - 1];
						object entity = io.ToDBEntity();
						table.Insert(context, entity);
					}

					for(int i = updatedOld.Count - 1; i >= 0; --i)
					{
						IdentifiedObject io = updatedOld[i];
						IEFTable table = tables[(int)ModelCodeHelper.GetTypeFromGID(io.GID) - 1];
						object entity = io.ToDBEntity();
						table.Update(context, entity);
					}

					for(int i = inserted.Count - 1; i >= 0; --i)
					{
						IdentifiedObject io = inserted[i];
						IEFTable table = tables[(int)ModelCodeHelper.GetTypeFromGID(io.GID) - 1];
						object entity = io.ToDBEntity();
						table.Delete(context, entity);
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
