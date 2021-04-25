using Common.Database;
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
	public enum ESCADAModelDatabaseTables { Analogs, Discretes }

	class SCADAModelEFDatabaseTransaction : IDatabaseTransaction<ESCADAModelDatabaseTables>
	{
		Dictionary<ESCADAModelDatabaseTables, ITableContext> tables;

		public SCADAModelEFDatabaseTransaction(SCADAModelDBContext context)
		{
			tables = new Dictionary<ESCADAModelDatabaseTables, ITableContext>(3)
			{
				{ ESCADAModelDatabaseTables.Analogs, new EFTableContext(new EFTable<AnalogDBModel>(), context) },
				{ ESCADAModelDatabaseTables.Discretes, new EFTableContext(new EFTable<DiscreteDBModel>(), context) }
			};
		}

		public ITableContext GetTable(ESCADAModelDatabaseTables table)
		{
			ITableContext tableContext;
			tables.TryGetValue(table, out tableContext);
			return tableContext;
		}
	}

	public class SCADAModelEFDatabase : IDatabase<ESCADAModelDatabaseTables>
	{
		public bool Transact(Func<IDatabaseTransaction<ESCADAModelDatabaseTables>, bool> f)
		{
			try
			{
				using(SCADAModelDBContext context = new SCADAModelDBContext())
				{
					if(f(new SCADAModelEFDatabaseTransaction(context)))
					{
						context.SaveChanges();
					}
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
