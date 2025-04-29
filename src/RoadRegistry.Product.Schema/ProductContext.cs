namespace RoadRegistry.Product.Schema;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BackOffice;
using BackOffice.Extracts.Dbase.RoadSegments;
using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
using GradeSeparatedJunctions;
using Microsoft.EntityFrameworkCore;
using Organizations;
using RoadNodes;
using RoadSegments;

public class ProductContext : RunnerDbContext<ProductContext>
{
    private RoadNetworkInfo _localRoadNetworkInfo;

    public ProductContext()
    {
    }

    // This needs to be DbContextOptions<T> for Autofac!
    public ProductContext(DbContextOptions<ProductContext> options)
        : base(options)
    {
    }

    public override string ProjectionStateSchema => WellKnownSchemas.ProductMetaSchema;

    public DbSet<GradeSeparatedJunctionRecord> GradeSeparatedJunctions { get; set; }
    public DbSet<OrganizationRecordV2> OrganizationsV2 { get; set; }
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
    public DbSet<RoadSegmentSurfaceAttributeRecord> RoadSegmentSurfaceAttributes { get; set; }
    public DbSet<RoadSegmentWidthAttributeRecord> RoadSegmentWidthAttributes { get; set; }

    public async ValueTask<RoadNetworkInfo> GetRoadNetworkInfo(CancellationToken token)
    {
        return _localRoadNetworkInfo ??=
            RoadNetworkInfo.Local.SingleOrDefault() ??
            await RoadNetworkInfo.SingleAsync(candidate => candidate.Id == BackOffice.RoadNetworkInfo.Identifier, token);
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
            .ToSqlQuery($"SELECT MIN([BoundingBox_MinimumX]) AS MinimumX, MAX([BoundingBox_MaximumX]) AS MaximumX, MIN([BoundingBox_MinimumY]) AS MinimumY, MAX([BoundingBox_MaximumY]) AS MaximumY FROM [{WellKnownSchemas.ProductSchema}].[RoadNode]");

        builder
            .Entity<RoadSegmentBoundingBox3D>()
            .HasNoKey()
            .ToSqlQuery($"SELECT MIN([BoundingBox_MinimumX]) AS MinimumX, MAX([BoundingBox_MaximumX]) AS MaximumX, MIN([BoundingBox_MinimumY]) AS MinimumY, MAX([BoundingBox_MaximumY]) AS MaximumY, MIN([BoundingBox_MinimumM]) AS MinimumM, MAX([BoundingBox_MaximumM]) AS MaximumM FROM [{WellKnownSchemas.ProductSchema}].[RoadSegment]");
    }
}
