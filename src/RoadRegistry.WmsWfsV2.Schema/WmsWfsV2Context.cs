namespace RoadRegistry.WmsWfsV2.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Records;

// SQL Server read model for the WMS v2 ("basisproduct"/"afgeleid product") export. Populated by the Marten-driven
// RoadNetworkChangesWmsWfsV2Projection (feature + dynamic-attribute tables) and by a one-time code-list sync. Table and
// column names mirror the shapefile (.dbf) product so the tables can be exported directly.
// Derives from RunnerDbContext for its ProjectionStates table: each inner WMS v2 projection records its processed event
// position there, committed in the same transaction as the product writes, so it can skip re-delivered events when the
// SQL Server write and the Marten progression commit diverge (RunnerDbContextRoadNetworkChangesProjection).
public class WmsWfsV2Context : RunnerDbContext<WmsWfsV2Context>
{
    public WmsWfsV2Context()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public WmsWfsV2Context(DbContextOptions<WmsWfsV2Context> options)
        : base(options)
    {
    }

    public override string ProjectionStateSchema => WellKnownSchemas.WmsWfsV2Schema;

    // Features
    public DbSet<RoadSegmentRecord> RoadSegments { get; set; }
    public DbSet<DerivedRoadSegmentRecord> DerivedRoadSegments { get; set; }
    public DbSet<RoadNodeRecord> RoadNodes { get; set; }
    public DbSet<GradeJunctionRecord> GradeJunctions { get; set; }
    public DbSet<GradeSeparatedJunctionRecord> GradeSeparatedJunctions { get; set; }
    public DbSet<EuropeanRoadRecord> EuropeanRoads { get; set; }
    public DbSet<NationalRoadRecord> NationalRoads { get; set; }

    // Internal caches (id -> label), fed by the streetname/organization events, used to resolve label columns
    public DbSet<StreetNameCacheRecord> StreetNameCache { get; set; }
    public DbSet<OrganizationCacheRecord> OrganizationCache { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseRoadRegistryInMemorySqlServer();
        }
    }

    internal static void ConfigureSqlServerOptions(SqlServerDbContextOptionsBuilder sqlServerOptions)
    {
        sqlServerOptions.UseNetTopologySuite();
    }
}
