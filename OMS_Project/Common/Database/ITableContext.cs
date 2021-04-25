using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Database
{
	public interface ITableContext
	{
		object Get(object key);
		List<object> GetList();
		List<object> Where(Func<object, bool> predicate);
		void Insert(object entity);
		void Delete(object entity);
		void Update(object entity);
	}
}
