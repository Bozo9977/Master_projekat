using Common.DataModel;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCADA
{
	class SCADAModelDBContext : DbContext
	{
		public DbSet<AnalogDBModel> Analogs { get; set; }
		public DbSet<DiscreteDBModel> Discretes { get; set; }

		public SCADAModelDBContext()
		{
			Database.SetInitializer(new Initializer());
			((IObjectContextAdapter)this).ObjectContext.CommandTimeout = int.MaxValue;
		}
	}

	class Initializer : DropCreateDatabaseIfModelChanges<SCADAModelDBContext>
	{ }
}
