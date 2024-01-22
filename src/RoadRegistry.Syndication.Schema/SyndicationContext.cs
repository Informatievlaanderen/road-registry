namespace RoadRegistry.Syndication.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;

public class SyndicationContext : RunnerDbContext<SyndicationContext>
{
    public SyndicationContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public SyndicationContext(DbContextOptions<SyndicationContext> options)
        : base(options)
    {
    }

    public DbSet<MunicipalityRecord> Municipalities { get; set; }
    public override string ProjectionStateSchema => WellKnownSchemas.SyndicationMetaSchema;

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }
}
