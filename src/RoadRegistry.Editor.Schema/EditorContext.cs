namespace RoadRegistry.Editor.Schema;

using BackOffice;
using BackOffice.Extracts.Dbase.Organizations;
using BackOffice.Extracts.Dbase.RoadSegments;
using BackOffice.Metrics;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using Extracts;
using GradeSeparatedJunctions;
using Microsoft.EntityFrameworkCore;
using RoadNetworkChanges;
using RoadNodes;
using RoadSegments;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class EditorContext : RunnerDbContext<EditorContext>
{
    private RoadNetworkInfo _localRoadNetworkInfo;

    public EditorContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public EditorContext(DbContextOptions<EditorContext> options)
        : base(options)
    {
    }

    public override string ProjectionStateSchema => WellKnownSchemas.EditorMetaSchema;

    #region Metrics
    public DbSet<EventProcessorMetricsRecord> EventProcessorMetrics { get; set; }
    #endregion

    public DbSet<ExtractRequestRecord> ExtractRequests { get; set; }
    public DbSet<ExtractRequestOverlapRecord> ExtractRequestOverlaps { get; set; }
    public DbSet<ExtractDownloadRecord> ExtractDownloads { get; set; }
    public DbSet<ExtractUploadRecord> ExtractUploads { get; set; }
    public DbSet<GradeSeparatedJunctionRecord> GradeSeparatedJunctions { get; set; }
    public DbSet<MunicipalityGeometry> MunicipalityGeometries { get; set; }
    public DbSet<OrganizationRecord> Organizations { get; set; }
    public DbSet<RoadNetworkChangeRequestBasedOnArchive> RoadNetworkChangeRequestsBasedOnArchive { get; set; }
    public DbSet<RoadNetworkChange> RoadNetworkChanges { get; set; }
    public DbSet<RoadNetworkInfo> RoadNetworkInfo { get; set; }
    public DbSet<RoadNetworkInfoSegmentCache> RoadNetworkInfoSegmentCache { get; set; }
    public DbSet<RoadNodeBoundingBox2D> RoadNodeBoundingBox { get; set; }
    public DbSet<RoadNodeRecord> RoadNodes { get; set; }
    public DbSet<RoadSegmentBoundingBox3D> RoadSegmentBoundingBox { get; set; }
    public DbSet<RoadSegmentEuropeanRoadAttributeRecord> RoadSegmentEuropeanRoadAttributes { get; set; }
    public DbSet<RoadSegmentLaneAttributeRecord> RoadSegmentLaneAttributes { get; set; }
    public DbSet<RoadSegmentNationalRoadAttributeRecord> RoadSegmentNationalRoadAttributes { get; set; }
    public DbSet<RoadSegmentNumberedRoadAttributeRecord> RoadSegmentNumberedRoadAttributes { get; set; }
    public DbSet<RoadSegmentRecord> RoadSegments { get; set; }
    public DbSet<RoadSegmentV2Record> RoadSegmentsV2 { get; set; }
    public DbSet<RoadSegmentSurfaceAttributeRecord> RoadSegmentSurfaceAttributes { get; set; }
    public DbSet<RoadSegmentWidthAttributeRecord> RoadSegmentWidthAttributes { get; set; }

    public async ValueTask<RoadNetworkInfo> GetRoadNetworkInfo(CancellationToken cancellationToken)
    {
        return _localRoadNetworkInfo ??=
            RoadNetworkInfo.Local.SingleOrDefault() ??
            await RoadNetworkInfo.SingleAsync(candidate => candidate.Id == BackOffice.RoadNetworkInfo.Identifier, cancellationToken);
    }

    protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseRoadRegistryInMemorySqlServer();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        OnModelQueryTypes(modelBuilder);
    }

    //HACK: Raw sql is not supported when running against in memory - this allows overriding and adjusting behavior

    protected virtual void OnModelQueryTypes(ModelBuilder builder)
    {
        builder
            .Entity<RoadNodeBoundingBox2D>()
            .HasNoKey()
            .ToSqlQuery($"SELECT MIN([BoundingBox_MinimumX]) AS MinimumX, MAX([BoundingBox_MaximumX]) AS MaximumX, MIN([BoundingBox_MinimumY]) AS MinimumY, MAX([BoundingBox_MaximumY]) AS MaximumY FROM [{WellKnownSchemas.EditorSchema}].[RoadNode]");

        builder
            .Entity<RoadSegmentBoundingBox3D>()
            .HasNoKey()
            .ToSqlQuery($"SELECT MIN([BoundingBox_MinimumX]) AS MinimumX, MAX([BoundingBox_MaximumX]) AS MaximumX, MIN([BoundingBox_MinimumY]) AS MinimumY, MAX([BoundingBox_MaximumY]) AS MaximumY, MIN([BoundingBox_MinimumM]) AS MinimumM, MAX([BoundingBox_MaximumM]) AS MaximumM FROM [{WellKnownSchemas.EditorSchema}].[RoadSegment]");
    }
}
