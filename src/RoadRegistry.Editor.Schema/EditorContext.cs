namespace RoadRegistry.Editor.Schema
{
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using GradeSeparatedJunctions;
    using Microsoft.EntityFrameworkCore;
    using Organizations;
    using RoadNodes;
    using RoadSegments;

    public class EditorContext : RunnerDbContext<EditorContext>
    {
        private RoadNetworkInfo _localRoadNetworkInfo;

        public override string ProjectionStateSchema => WellknownSchemas.EditorMetaSchema;

        public DbSet<RoadNodeRecord> RoadNodes { get; set; }
        public DbSet<RoadSegmentRecord> RoadSegments { get; set; }
        public DbSet<RoadSegmentLaneAttributeRecord> RoadSegmentLaneAttributes { get; set; }
        public DbSet<RoadSegmentWidthAttributeRecord> RoadSegmentWidthAttributes { get; set; }
        public DbSet<RoadSegmentSurfaceAttributeRecord> RoadSegmentSurfaceAttributes { get; set; }
        public DbSet<RoadSegmentEuropeanRoadAttributeRecord> RoadSegmentEuropeanRoadAttributes { get; set; }
        public DbSet<RoadSegmentNationalRoadAttributeRecord> RoadSegmentNationalRoadAttributes { get; set; }
        public DbSet<RoadSegmentNumberedRoadAttributeRecord> RoadSegmentNumberedRoadAttributes { get; set; }
        public DbSet<GradeSeparatedJunctionRecord> GradeSeparatedJunctions { get; set; }
        public DbSet<OrganizationRecord> Organizations { get; set; }
        public DbSet<RoadNetworkInfo> RoadNetworkInfo { get; set; }
        public DbSet<RoadNodeBoundingBox2D> RoadNodeBoundingBox { get; set; }
        public DbSet<RoadSegmentBoundingBox3D> RoadSegmentBoundingBox { get; set; }
        public DbSet<RoadNetworkChange> RoadNetworkChanges { get; set; }

        public async ValueTask<RoadNetworkInfo> GetRoadNetworkInfo(CancellationToken token)
        {
            return _localRoadNetworkInfo ??=
                RoadNetworkInfo.Local.SingleOrDefault() ??
                await RoadNetworkInfo.SingleAsync(candidate => candidate.Id == 0, token);
        }

        public EditorContext() {}

        // This needs to be DbContextOptions<T> for Autofac!
        public EditorContext(DbContextOptions<EditorContext> options)
            : base(options) { }

        protected override void OnConfiguringOptionsBuilder(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder.UseSqlServer(@"Server=(localdb)\mssqllocaldb;Database=EFProviders.InMemory.RoadRegistry.RoadRegistryContext;Trusted_Connection=True;");

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            OnModelQueryTypes(builder);
        }

        //HACK: Raw sql is not supported when running against in memory - this allows overriding and adjusting behavior

        protected virtual void OnModelQueryTypes(ModelBuilder builder)
        {
            builder
                .Entity<RoadNodeBoundingBox2D>()
                .HasNoKey()
                .ToQuery(() => RoadNodeBoundingBox.FromSqlRaw("SELECT MIN([BoundingBox_MinimumX]) AS MinimumX, MAX([BoundingBox_MaximumX]) AS MaximumX, MIN([BoundingBox_MinimumY]) AS MinimumY, MAX([BoundingBox_MaximumY]) AS MaximumY FROM [" + WellknownSchemas.EditorSchema + "].[RoadNode]"));

            builder
                .Entity<RoadSegmentBoundingBox3D>()
                .HasNoKey()
                .ToQuery(() => RoadSegmentBoundingBox.FromSqlRaw("SELECT MIN([BoundingBox_MinimumX]) AS MinimumX, MAX([BoundingBox_MaximumX]) AS MaximumX, MIN([BoundingBox_MinimumY]) AS MinimumY, MAX([BoundingBox_MaximumY]) AS MaximumY, MIN([BoundingBox_MinimumM]) AS MinimumM, MAX([BoundingBox_MaximumM]) AS MaximumM FROM [" + WellknownSchemas.EditorSchema + "].[RoadSegment]"));
        }
    }
}
