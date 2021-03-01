using SCADA_Common.DB_Model;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA_Common
{
    public class SCADA_DBContext: DbContext
    {
        public DbSet<PointItemDB> PointItems { get; set; }
        public DbSet<AnalogPointItemDB> AnalogPointItems { get; set; }
        public DbSet<DiscretePointItemDB> DiscretePointItems { get; set; }

        public SCADA_DBContext()
        {
            Database.SetInitializer(new SCADAInitializer());
            ((IObjectContextAdapter)this).ObjectContext.CommandTimeout = int.MaxValue;
        }
    }

    class SCADAInitializer : DropCreateDatabaseIfModelChanges<SCADA_DBContext>
    { }

}
