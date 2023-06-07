namespace RoadRegistry.Wfs.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;

public class WfsContext : RunnerDbContext<WfsContext>
{
    // Temporarily unsupported
    // public DbSet<GradeSeparatedJunctionRecord> GradeSeparatedJunctions { get; set; }
    public WfsContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public WfsContext(DbContextOptions<WfsContext> options)
        : base(options)
    {
    }

    public override string ProjectionStateSchema => WellknownSchemas.WfsMetaSchema;
    public DbSet<RoadNodeRecord> RoadNodes { get; set; }
    public DbSet<RoadSegmentRecord> RoadSegments { get; set; }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
    }
}
