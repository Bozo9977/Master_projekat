using Common.Database;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.EntityFramework
{
	public class EFTableContext : ITableContext
	{
		IEFTable table;
		DbContext context;

		public EFTableContext(IEFTable table, DbContext context)
		{
			this.table = table;
			this.context = context;
		}

		public object Get(object key)
		{
			return table.Get(context, key);
		}

		public List<object> GetList()
		{
			return table.GetList(context);
		}

		public List<object> Where(Func<object, bool> predicate)
		{
			return table.Where(context, predicate);
		}

		public void Insert(object entity)
		{
			table.Insert(context, entity);
		}

		public void Delete(object entity)
		{
			table.Delete(context, entity);
		}

		public void Update(object entity)
		{
			table.Update(context, entity);
		}
	}
}
