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
public class PbsContext : RunnerDbContext<PbsContext>, ISchemaScopedDbContext
{
    public PbsContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    //
    // The only constructor taking options, and it has to stay that way: EF's DbContextFactory builds an
    // activator for this type and refuses one with a second constructor it could use. The schema a context
    // is scoped to therefore travels on the options - see UseSchema - and not as a parameter of its own.
    public PbsContext(DbContextOptions<PbsContext> options)
        : base(options)
    {
        Schema = options.FindSchema() ?? WellKnownSchemas.PbsSchema;
    }

    // The schema this context reads and writes: the production one, or the shadow copy a rebuild fills
    // while the live one keeps serving. Everything else about the context is identical, which is the point -
    // one model, one set of projections.
    public string Schema { get; } = WellKnownSchemas.PbsSchema;

    public override string ProjectionStateSchema => Schema;

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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        if (Schema != WellKnownSchemas.PbsSchema)
        {
            modelBuilder.MapToSchema(Schema);
        }
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);

        // Here rather than where the options are built, so no instance can be handed a model that was
        // cached for another schema.
        optionsBuilder.UseSchemaAwareModelCache();

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
