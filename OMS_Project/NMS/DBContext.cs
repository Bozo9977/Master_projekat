using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.DataModel;

namespace NMS
{
	public class DBContext : DbContext
	{
		public DbSet<ACLineSegmentDBModel> ACLineSegments { get; set; }
		public DbSet<AnalogDBModel> Analogs { get; set; }
		public DbSet<BaseVoltageDBModel> BaseVoltages { get; set; }
		public DbSet<BreakerDBModel> Breakers { get; set; }
		public DbSet<ConnectivityNodeDBModel> ConnectivityNodes { get; set; }
		public DbSet<DisconnectorDBModel> Disconnectors { get; set; }
		public DbSet<DiscreteDBModel> Discretes { get; set; }
		public DbSet<DistributionGeneratorDBModel> DistributionGenerators { get; set; }
		public DbSet<EnergyConsumerDBModel> EnergyConsumers { get; set; }
		public DbSet<EnergySourceDBModel> EnergySources { get; set; }
		public DbSet<PowerTransformerDBModel> PowerTransformers { get; set; }
		public DbSet<RatioTapChangerDBModel> RatioTapChangers { get; set; }
		public DbSet<RecloserDBModel> Reclosers { get; set; }
		public DbSet<TerminalDBModel> Terminals { get; set; }
		public DbSet<TransformerWindingDBModel> TransformerWindings { get; set; }

		public DBContext()
		{
			Database.SetInitializer(new Initializer());
			((IObjectContextAdapter)this).ObjectContext.CommandTimeout = int.MaxValue;
		}
	}

	class Initializer : DropCreateDatabaseIfModelChanges<DBContext>
	{ }
}
