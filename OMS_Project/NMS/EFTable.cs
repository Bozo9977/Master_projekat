using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NMS
{
	public class EFTable<TEntity> : IEFTable where TEntity : class
	{
		public virtual void Delete(DbContext context, TEntity entity)
		{
			DbSet<TEntity> set = context.Set<TEntity>();
			set.Attach(entity);
			set.Remove(entity);
		}

		public virtual TEntity Get(DbContext context, params object[] key)
		{
			return context.Set<TEntity>().Find(key);
		}

		public virtual List<TEntity> GetList(DbContext context)
		{
			return context.Set<TEntity>().ToList();
		}

		public virtual List<TEntity> Where(DbContext context, Func<TEntity, bool> predicate)
		{
			return context.Set<TEntity>().Where(predicate).ToList();
		}

		public virtual void Insert(DbContext context, TEntity entity)
		{
			context.Set<TEntity>().Add(entity);
		}

		public virtual void Update(DbContext context, TEntity entity)
		{
			context.Set<TEntity>().Attach(entity);
			context.Entry(entity).State = System.Data.Entity.EntityState.Modified;
		}

		object IEFTable.Get(DbContext context, params object[] key)
		{
			return Get(context, key);
		}

		List<object> IEFTable.GetList(DbContext context)
		{
			return GetList(context).Cast<object>().ToList();
		}

		List<object> IEFTable.Where(DbContext context, Func<object, bool> predicate) 
		{
			return Where(context, predicate).Cast<object>().ToList();
		}

		void IEFTable.Insert(DbContext context, object entity)
		{
			Insert(context, (TEntity)entity);
		}

		void IEFTable.Delete(DbContext context, object entity)
		{
			Delete(context, (TEntity)entity);
		}

		void IEFTable.Update(DbContext context, object entity)
		{
			Update(context, (TEntity)entity);
		}
	}
}
