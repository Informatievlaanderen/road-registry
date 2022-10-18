namespace RoadRegistry.Wms.Schema;

using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Hosts;
using Microsoft.EntityFrameworkCore;

public class WmsContext : RunnerDbContext<WmsContext>
{
    public WmsContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public WmsContext(DbContextOptions<WmsContext> options)
        : base(options)
    {
    }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");
    }

    public override string ProjectionStateSchema => WellknownSchemas.WmsMetaSchema;

    // public DbSet<RoadSegmentDenormRecord> RoadSegments { get; set; }
    public DbSet<RoadSegmentRecord> RoadSegments { get; set; }
}
