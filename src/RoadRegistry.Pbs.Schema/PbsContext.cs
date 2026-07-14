namespace RoadRegistry.Pbs.Schema;

using BackOffice;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Records;

// SQL Server read model for the PBS ("basisproduct"/"afgeleid product") export. Populated by the Marten-driven
// RoadNetworkChangesPbsProjection (feature + dynamic-attribute tables) and by a one-time code-list sync. Table and
// column names mirror the shapefile (.dbf) product so the tables can be exported directly.
// Derives from RunnerDbContext for its ProjectionStates table: each inner PBS projection records its processed event
// position there, committed in the same transaction as the product writes, so it can skip re-delivered events when the
// SQL Server write and the Marten progression commit diverge (RunnerDbContextRoadNetworkChangesProjection).
public class PbsContext : RunnerDbContext<PbsContext>
{
    public PbsContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public PbsContext(DbContextOptions<PbsContext> options)
        : base(options)
    {
    }

    public override string ProjectionStateSchema => WellKnownSchemas.PbsSchema;

    // Features
    public DbSet<RoadSegmentRecord> RoadSegments { get; set; }
    public DbSet<DerivedRoadSegmentRecord> DerivedRoadSegments { get; set; }
    public DbSet<RoadNodeRecord> RoadNodes { get; set; }
    public DbSet<GradeJunctionRecord> GradeJunctions { get; set; }
    public DbSet<GradeSeparatedJunctionRecord> GradeSeparatedJunctions { get; set; }
    public DbSet<EuropeanRoadRecord> EuropeanRoads { get; set; }
    public DbSet<NationalRoadRecord> NationalRoads { get; set; }

    // Dynamic attributes of a road segment
    public DbSet<RoadSegmentMorphologyAttributeRecord> RoadSegmentMorphologyAttributes { get; set; }
    public DbSet<RoadSegmentStreetNameAttributeRecord> RoadSegmentStreetNameAttributes { get; set; }
    public DbSet<RoadSegmentAccessRestrictionAttributeRecord> RoadSegmentAccessRestrictionAttributes { get; set; }
    public DbSet<RoadSegmentCarTrafficDirectionAttributeRecord> RoadSegmentCarTrafficDirectionAttributes { get; set; }
    public DbSet<RoadSegmentBikeTrafficDirectionAttributeRecord> RoadSegmentBikeTrafficDirectionAttributes { get; set; }
    public DbSet<RoadSegmentPedestrianTrafficDirectionAttributeRecord> RoadSegmentPedestrianTrafficDirectionAttributes { get; set; }
    public DbSet<RoadSegmentMaintenanceAuthorityAttributeRecord> RoadSegmentMaintenanceAuthorityAttributes { get; set; }
    public DbSet<RoadSegmentCategoryAttributeRecord> RoadSegmentCategoryAttributes { get; set; }
    public DbSet<RoadSegmentSurfaceTypeAttributeRecord> RoadSegmentSurfaceTypeAttributes { get; set; }

    // Code lists (synced from the V2 domain; Wegbeheerder comes from the organization projection)
    public DbSet<RoadNodeTypeCodeListRecord> RoadNodeTypeCodeList { get; set; }
    public DbSet<GradeSeparatedJunctionTypeCodeListRecord> GradeSeparatedJunctionTypeCodeList { get; set; }
    public DbSet<RoadSegmentSideCodeListRecord> RoadSegmentSideCodeList { get; set; }
    public DbSet<RoadSegmentMethodCodeListRecord> RoadSegmentMethodCodeList { get; set; }
    public DbSet<RoadSegmentMorphologyCodeListRecord> RoadSegmentMorphologyCodeList { get; set; }
    public DbSet<RoadSegmentDirectionCodeListRecord> RoadSegmentDirectionCodeList { get; set; }
    public DbSet<RoadSegmentStatusCodeListRecord> RoadSegmentStatusCodeList { get; set; }
    public DbSet<RoadSegmentAccessRestrictionCodeListRecord> RoadSegmentAccessRestrictionCodeList { get; set; }
    public DbSet<RoadSegmentSurfaceTypeCodeListRecord> RoadSegmentSurfaceTypeCodeList { get; set; }
    public DbSet<RoadSegmentMaintenanceAuthorityCodeListRecord> RoadSegmentMaintenanceAuthorityCodeList { get; set; }
    public DbSet<RoadSegmentCategoryCodeListRecord> RoadSegmentCategoryCodeList { get; set; }

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
