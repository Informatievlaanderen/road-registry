namespace RoadRegistry.Projections.Tests.Projections.Pbs.GradeJunction;

using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.Pbs.Projections;
using RoadRegistry.Pbs.Schema.Records;
using RoadRegistry.RoadNode.Events.V1;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.ScopedRoadNetwork.Events.V1;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadSegmentV1 = RoadRegistry.RoadSegment.Events.V1;
using RoadSegmentV2 = RoadRegistry.RoadSegment.Events.V2;

public class GradeJunctionPbsProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private PbsProjectionScenario Scenario() =>
        new(factory => new[] { new GradeJunctionPbsProjection(factory) });

    private ProvenanceData Provenance => new(_testData.Provenance);

    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        // This projection handles the two GradeJunction V2 events plus the segment-geometry events that force the
        // junction point to be recomputed; everything else is excluded.
        var excludeEventTypes = new[]
        {
            // RoadNode V1
            typeof(ImportedRoadNode), typeof(RoadNodeAdded), typeof(RoadNodeModified), typeof(RoadNodeRemoved),
            // RoadNode V2
            typeof(RoadNodeWasAdded), typeof(RoadNodeTypeWasChanged), typeof(RoadNodeWasModified),
            typeof(RoadNodeWasMigrated), typeof(RoadNodeWasRemoved), typeof(RoadNodeWasRemovedBecauseOfMigration),
            // RoadSegment V1 (except RoadSegmentModified / RoadSegmentGeometryModified, which are handled)
            typeof(RoadSegmentV1.ImportedRoadSegment), typeof(RoadSegmentV1.OutlinedRoadSegmentRemoved),
            typeof(RoadSegmentV1.RoadSegmentAdded), typeof(RoadSegmentV1.RoadSegmentAddedToEuropeanRoad),
            typeof(RoadSegmentV1.RoadSegmentAddedToNationalRoad), typeof(RoadSegmentV1.RoadSegmentAddedToNumberedRoad),
            typeof(RoadSegmentV1.RoadSegmentAttributesModified), typeof(RoadSegmentV1.RoadSegmentRemoved),
            typeof(RoadSegmentV1.RoadSegmentRemovedFromEuropeanRoad), typeof(RoadSegmentV1.RoadSegmentRemovedFromNationalRoad),
            typeof(RoadSegmentV1.RoadSegmentRemovedFromNumberedRoad), typeof(RoadSegmentV1.RoadSegmentStreetNamesChanged),
            // RoadSegment V2 (except the geometry events handled above)
            typeof(RoadSegmentV2.OutlinedRoadSegmentWasAdded), typeof(RoadSegmentV2.RoadSegmentStreetNameIdWasChanged),
            typeof(RoadSegmentV2.RoadSegmentWasAdded), typeof(RoadSegmentV2.RoadSegmentWasAddedToEuropeanRoad),
            typeof(RoadSegmentV2.RoadSegmentWasAddedToNationalRoad), typeof(RoadSegmentV2.RoadSegmentWasRemoved),
            typeof(RoadSegmentV2.RoadSegmentWasRemovedBecauseOfMigration), typeof(RoadSegmentV2.RoadSegmentWasRemovedFromEuropeanRoad),
            typeof(RoadSegmentV2.RoadSegmentWasRemovedFromNationalRoad), typeof(RoadSegmentV2.RoadSegmentWasRetired),
            typeof(RoadSegmentV2.RoadSegmentWasRetiredBecauseOfMerger), typeof(RoadSegmentV2.RoadSegmentWasRetiredBecauseOfSplit),
            // GradeSeparatedJunction V1
            typeof(ImportedGradeSeparatedJunction), typeof(GradeSeparatedJunctionAdded),
            typeof(GradeSeparatedJunctionModified), typeof(GradeSeparatedJunctionRemoved),
            // GradeSeparatedJunction V2
            typeof(GradeSeparatedJunctionWasAdded), typeof(GradeSeparatedJunctionWasModified),
            typeof(GradeSeparatedJunctionWasRemoved), typeof(GradeSeparatedJunctionWasRemovedBecauseOfMigration),
            // Organization V2
            typeof(OrganizationWasImported), typeof(OrganizationWasCreated),
            typeof(OrganizationWasModified), typeof(OrganizationWasRemoved),
            // StreetName V2
            typeof(StreetNameWasCreated), typeof(StreetNameWasModified),
            typeof(StreetNameWasRemoved), typeof(StreetNameWasRenamed),
            // ScopedRoadNetwork
            typeof(RoadNetworkChangesAccepted), typeof(MunicipalityWasMigrated),
            typeof(RoadNetworkWasChangedBecauseOfExtract)
        };

        PbsProjectionEventCoverage.AssertHandledExactlyOnce(new GradeJunctionPbsProjection(null!), excludeEventTypes);
    }

    // A junction carries no geometry of its own; its point is the first intersection of the two linked segments.
    private static Geometry Line((double X, double Y) from, (double X, double Y) to) =>
        new LineString([new Coordinate(from.X, from.Y), new Coordinate(to.X, to.Y)])
        {
            SRID = WellknownSrids.Lambert08
        };

    private async Task SeedCrossingSegmentsAsync(PbsProjectionScenario scenario)
    {
        await scenario.SeedAsync(context =>
        {
            context.RoadSegments.Add(new RoadSegmentRecord { WS_OIDN = 1, GEOMETRIE = Line((0, 0), (100, 100)) });
            context.RoadSegments.Add(new RoadSegmentRecord { WS_OIDN = 2, GEOMETRIE = Line((0, 100), (100, 0)) });
            return Task.CompletedTask;
        });
    }

    [Fact]
    public async Task WhenGradeJunctionWasAdded_ThenStoredWithIntersectionGeometry()
    {
        var scenario = Scenario();
        await SeedCrossingSegmentsAsync(scenario);

        await scenario.GivenAsync(new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(1),
            RoadSegmentId2 = new RoadSegmentId(2),
            Provenance = Provenance
        });

        var junction = await scenario.Find<GradeJunctionRecord>(1);
        Assert.NotNull(junction);
        Assert.Equal(1, junction!.WS1_OIDN);
        Assert.Equal(2, junction.WS2_OIDN);
        Assert.NotNull(junction.GEOMETRIE);
        Assert.Equal(50.0, junction.GEOMETRIE.Coordinate.X, 3);
        Assert.Equal(50.0, junction.GEOMETRIE.Coordinate.Y, 3);
    }

    [Fact]
    public async Task WhenLinkedSegmentsDoNotIntersect_ThenGeometryIsNull()
    {
        var scenario = Scenario();
        await scenario.SeedAsync(context =>
        {
            context.RoadSegments.Add(new RoadSegmentRecord { WS_OIDN = 1, GEOMETRIE = Line((0, 0), (100, 0)) });
            context.RoadSegments.Add(new RoadSegmentRecord { WS_OIDN = 2, GEOMETRIE = Line((0, 100), (100, 100)) });
            return Task.CompletedTask;
        });

        await scenario.GivenAsync(new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(1),
            RoadSegmentId2 = new RoadSegmentId(2),
            Provenance = Provenance
        });

        var junction = await scenario.Find<GradeJunctionRecord>(1);
        Assert.NotNull(junction);
        Assert.Null(junction!.GEOMETRIE);
    }

    [Fact]
    public async Task WhenLinkedSegmentGeometryChanges_ThenIntersectionRecomputed()
    {
        var scenario = Scenario();
        await SeedCrossingSegmentsAsync(scenario);

        await scenario.GivenAsync(new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(1),
            RoadSegmentId2 = new RoadSegmentId(2),
            Provenance = Provenance
        });

        // Move segment 2 so it now crosses segment 1 ((0,0)-(100,100)) at (25,25) instead of (50,50).
        await scenario.SeedAsync(async context =>
        {
            var segment = await context.RoadSegments.FindAsync(2);
            segment!.GEOMETRIE = Line((0, 50), (50, 0));
        });

        // The recalculation reads the stored segment geometry; the event's own geometry payload is ignored.
        await scenario.GivenAsync(new RoadRegistry.RoadSegment.Events.V2.RoadSegmentGeometryWasModified
        {
            RoadSegmentId = new RoadSegmentId(2),
            Geometry = _testData.MultiLineString1.ToRoadSegmentGeometry(),
            StartNodeId = null,
            EndNodeId = null,
            Provenance = Provenance
        });

        var junction = await scenario.Find<GradeJunctionRecord>(1);
        Assert.Equal(25.0, junction!.GEOMETRIE.Coordinate.X, 3);
        Assert.Equal(25.0, junction.GEOMETRIE.Coordinate.Y, 3);
    }

    [Fact]
    public async Task WhenGradeJunctionWasRemoved_ThenDeleted()
    {
        var scenario = Scenario();
        await SeedCrossingSegmentsAsync(scenario);

        await scenario.GivenAsync(new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(1),
            RoadSegmentId2 = new RoadSegmentId(2),
            Provenance = Provenance
        });
        await scenario.GivenAsync(new GradeJunctionWasRemoved
        {
            GradeJunctionId = new GradeJunctionId(1),
            Provenance = Provenance
        });

        Assert.Null(await scenario.Find<GradeJunctionRecord>(1));
    }
}
