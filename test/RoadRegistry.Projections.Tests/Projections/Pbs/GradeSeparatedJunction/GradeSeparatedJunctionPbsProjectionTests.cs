namespace RoadRegistry.Projections.Tests.Projections.Pbs.GradeSeparatedJunction;

using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using RoadRegistry.GradeJunction.Events.V2;
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
using GradeSeparatedJunctionV1 = RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadSegmentV1 = RoadRegistry.RoadSegment.Events.V1;
using RoadSegmentV2 = RoadRegistry.RoadSegment.Events.V2;

public class GradeSeparatedJunctionPbsProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private PbsProjectionScenario Scenario() =>
        new(factory => new[] { new GradeSeparatedJunctionPbsProjection(factory) });

    private ProvenanceData Provenance => new(_testData.Provenance);

    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        // This projection handles all GradeSeparatedJunction V1 + V2 events plus the segment-geometry events that force
        // the junction point to be recomputed; everything else is excluded.
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
            // GradeJunction V2
            typeof(GradeJunctionWasAdded), typeof(GradeJunctionWasRemoved),
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

        PbsProjectionEventCoverage.AssertHandledExactlyOnce(new GradeSeparatedJunctionPbsProjection(null!), excludeEventTypes);
    }

    private static Geometry Line((double X, double Y) from, (double X, double Y) to) =>
        new LineString(new[] { new Coordinate(from.X, from.Y), new Coordinate(to.X, to.Y) })
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
    public async Task WhenGradeSeparatedJunctionWasAdded_ThenStoredWithTypeAndIntersectionGeometry()
    {
        var scenario = Scenario();
        await SeedCrossingSegmentsAsync(scenario);
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(2),
            Type = type,
            Provenance = Provenance
        });

        var junction = await scenario.Find<GradeSeparatedJunctionRecord>(1);
        Assert.NotNull(junction);
        Assert.Equal(1, junction!.ON_WS_OIDN);
        Assert.Equal(2, junction.BO_WS_OIDN);
        Assert.Equal(type.Translation.Identifier, junction.TYPE);
        Assert.Equal(type.Translation.Name, junction.LBLTYPE);
        Assert.NotNull(junction.GEOMETRIE);
        Assert.Equal(50.0, junction.GEOMETRIE.Coordinate.X, 3);
        Assert.Equal(50.0, junction.GEOMETRIE.Coordinate.Y, 3);
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasModified_ThenTypeUpdated()
    {
        var scenario = Scenario();
        await SeedCrossingSegmentsAsync(scenario);
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();
        var newType = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(2),
            Type = type,
            Provenance = Provenance
        });
        await scenario.GivenAsync(new GradeSeparatedJunctionWasModified
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = null,
            UpperRoadSegmentId = null,
            Type = newType,
            Provenance = Provenance
        });

        var junction = await scenario.Find<GradeSeparatedJunctionRecord>(1);
        Assert.Equal(newType.Translation.Identifier, junction!.TYPE);
        Assert.Equal(newType.Translation.Name, junction.LBLTYPE);
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasRemoved_ThenDeleted()
    {
        var scenario = Scenario();
        await SeedCrossingSegmentsAsync(scenario);
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(2),
            Type = type,
            Provenance = Provenance
        });
        await scenario.GivenAsync(new GradeSeparatedJunctionWasRemoved
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            Provenance = Provenance
        });

        Assert.Null(await scenario.Find<GradeSeparatedJunctionRecord>(1));
    }
}
