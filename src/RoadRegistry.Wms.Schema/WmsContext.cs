namespace RoadRegistry.Wms.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
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

    public override string ProjectionStateSchema => WellknownSchemas.WmsMetaSchema;

    public DbSet<RoadSegmentRecord> RoadSegments { get; set; }
    public DbSet<RoadSegmentEuropeanRoadAttributeRecord> RoadSegmentEuropeanRoadAttributes { get; set; }
    public DbSet<RoadSegmentNationalRoadAttributeRecord> RoadSegmentNationalRoadAttributes { get; set; }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }
}
