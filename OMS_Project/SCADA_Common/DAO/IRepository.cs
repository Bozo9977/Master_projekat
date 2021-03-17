using Common.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common.DAO
{
    public interface IRepository<TEntity> where TEntity : class
    {
        TEntity FindById(object id);
        List<TEntity> GetAll();
        void Insert(TEntity entity);
        void Delete(object id);
        void Update(TEntity entityToUpdate);
    }
}
