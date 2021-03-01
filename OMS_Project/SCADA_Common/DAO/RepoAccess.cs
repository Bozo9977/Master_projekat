using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.DAO
{
    public class RepoAccess<TEntity> : IRepository<TEntity> where TEntity : class
    {
        public RepoAccess()
        {

        }


        public virtual void Delete(object id)
        {
            using (SCADA_DBContext db = new SCADA_DBContext())
            {
                try
                {
                    DbSet<TEntity> dbSet = db.Set<TEntity>();
                    TEntity entityToDelete = db.Set<TEntity>().Find(id);
                    db.Entry(entityToDelete).State = EntityState.Deleted;
                    db.SaveChanges();

                }
                catch (Exception e)
                {
                    Console.WriteLine("Message:\n" + e.Message + "\n\nTrace:\n" + e.StackTrace + "\n\nInner:\n" + e.InnerException);
                }
            }
        }

        public virtual TEntity FindById(object id)
        {
            using (SCADA_DBContext db = new SCADA_DBContext())
            {
                return db.Set<TEntity>().Find(id);
            }
        }

        public virtual List<TEntity> GetAll()
        {
            using (SCADA_DBContext db = new SCADA_DBContext())
            {
                return db.Set<TEntity>().ToList();
            }
        }

        public virtual void Insert(TEntity entity)
        {
            using (SCADA_DBContext db = new SCADA_DBContext())
            {
                try
                {
                    db.Set<TEntity>().Add(entity);
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Message:\n" + e.Message + "\n\nTrace:\n" + e.StackTrace + "\n\nInner:\n" + e.InnerException);
                }
            }
        }

        public virtual void Update(TEntity entityToUpdate)
        {
            using (SCADA_DBContext db = new SCADA_DBContext())
            {
                try
                {
                    db.Set<TEntity>().Attach(entityToUpdate);
                    db.Entry(entityToUpdate).State = EntityState.Modified;
                    db.SaveChanges();
                }
                catch (Exception e)
                {
                    Console.WriteLine("Message:\n" + e.Message + "\n\nTrace:\n" + e.StackTrace + "\n\nInner:\n" + e.InnerException);
                }
            }
        }

    }
}
