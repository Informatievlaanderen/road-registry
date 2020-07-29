namespace RoadRegistry.Syndication.Schema
{
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Microsoft.EntityFrameworkCore;

    public class SyndicationContext : RunnerDbContext<SyndicationContext>
    {
        public override string ProjectionStateSchema => WellknownSchemas.SyndicationMetaSchema;

        public DbSet<MunicipalityRecord> Municipalities { get; set; }
        public DbSet<StreetNameRecord> StreetNames { get; set; }
        public SyndicationContext() {}

        // This needs to be DbContextOptions<T> for Autofac!
        public SyndicationContext(DbContextOptions<SyndicationContext> options)
            : base(options) { }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
    }
}
