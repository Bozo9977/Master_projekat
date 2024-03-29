﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.EntityFramework
{
	public interface IEFTable
	{
		object Get(DbContext context, object key);
		List<object> GetList(DbContext context);
		List<object> Where(DbContext context, Func<object, bool> predicate);
		void Insert(DbContext context, object entity);
		void Delete(DbContext context, object entity);
		void Update(DbContext context, object entity);
	}
}
