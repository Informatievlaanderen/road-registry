namespace RoadRegistry.Projections.Oslo
{
    using Aiv.Vbr.ProjectionHandling.Runner;
    using Infrastructure;
    using Microsoft.EntityFrameworkCore;
    using RoadList;

    public class OsloContext : RunnerDbContext<OsloContext>
    {
        public override string ProjectionStateSchema => Schema.ProjectionMetaData;

        public DbSet<RoadListItem> RoadList { get; set; }
        public DbSet<Road.Road> Roads { get; set; }

        public OsloContext() { }

        // This needs to be DbContextOptions<T> for Autofac!
        public OsloContext(DbContextOptions<OsloContext> options)
            : base(options) { }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
    }
}
