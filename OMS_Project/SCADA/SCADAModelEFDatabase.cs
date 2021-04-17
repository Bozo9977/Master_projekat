using Common.DataModel;
using Common.EntityFramework;
using Common.GDA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA
{
	public class SCADAModelEFDatabase : ISCADAModelDatabase
	{
		Dictionary<DMSType, IEFTable> tables;

		public SCADAModelEFDatabase()
		{
			tables = new Dictionary<DMSType, IEFTable> { { DMSType.Analog, new EFTable<AnalogDBModel>() }, { DMSType.Discrete, new EFTable<DiscreteDBModel>() } };
		}

		public List<IdentifiedObject> GetList(DMSType type)
		{
			List<IdentifiedObject> l = new List<IdentifiedObject>();
			IEFTable table;

			if(!tables.TryGetValue(type, out table))
			{
				return null;
			}

			try
			{
				using(SCADAModelDBContext context = new SCADAModelDBContext())
				{
					foreach(object entity in table.GetList(context))
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
				using(SCADAModelDBContext context = new SCADAModelDBContext())
				{
					foreach(IdentifiedObject io in inserted)
					{
						IEFTable table;

						if(!tables.TryGetValue(ModelCodeHelper.GetTypeFromGID(io.GID), out table))
						{
							return false;
						}

						object entity = io.ToDBEntity();
						table.Insert(context, entity);
					}

					foreach(IdentifiedObject io in updatedNew)
					{
						IEFTable table;

						if(!tables.TryGetValue(ModelCodeHelper.GetTypeFromGID(io.GID), out table))
						{
							return false;
						}

						object entity = io.ToDBEntity();
						table.Update(context, entity);
					}

					foreach(IdentifiedObject io in deleted)
					{
						IEFTable table;

						if(!tables.TryGetValue(ModelCodeHelper.GetTypeFromGID(io.GID), out table))
						{
							return false;
						}

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
				using(SCADAModelDBContext context = new SCADAModelDBContext())
				{
					for(int i = deleted.Count - 1; i >= 0; --i)
					{
						IdentifiedObject io = deleted[i];
						IEFTable table;

						if(!tables.TryGetValue(ModelCodeHelper.GetTypeFromGID(io.GID), out table))
						{
							return false;
						}

						object entity = io.ToDBEntity();
						table.Insert(context, entity);
					}

					for(int i = updatedOld.Count - 1; i >= 0; --i)
					{
						IdentifiedObject io = updatedOld[i];
						IEFTable table;

						if(!tables.TryGetValue(ModelCodeHelper.GetTypeFromGID(io.GID), out table))
						{
							return false;
						}

						object entity = io.ToDBEntity();
						table.Update(context, entity);
					}

					for(int i = inserted.Count - 1; i >= 0; --i)
					{
						IdentifiedObject io = inserted[i];
						IEFTable table;

						if(!tables.TryGetValue(ModelCodeHelper.GetTypeFromGID(io.GID), out table))
						{
							return false;
						}

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
