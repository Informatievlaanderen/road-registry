namespace RoadRegistry.Projections.Tests.Projections.WmsWfsV2.GradeJunction;

using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.WmsWfsV2.Projections;
using RoadRegistry.WmsWfsV2.Schema.Records;
using RoadRegistry.RoadNode.Events.V1;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.ScopedRoadNetwork.Events.V1;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.ValueObjects;
using RoadSegmentV1 = RoadRegistry.RoadSegment.Events.V1;
using RoadSegmentV2 = RoadRegistry.RoadSegment.Events.V2;

public class GradeJunctionWmsWfsV2ProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private WmsWfsV2ProjectionScenario Scenario() =>
        new(factory => new[] { new GradeJunctionWmsWfsV2Projection(factory) });

    private ProvenanceData Provenance => new(_testData.Provenance);

    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        // This projection handles exactly the three GradeJunction V2 events (add, geometry-changed, remove); the junction
        // point now arrives on those events, so RoadSegment events are no longer handled. Everything else is excluded.
        var excludeEventTypes = new[]
        {
            // RoadNode V1
            typeof(ImportedRoadNode), typeof(RoadNodeAdded), typeof(RoadNodeModified), typeof(RoadNodeRemoved),
            // RoadNode V2
            typeof(RoadNodeWasAdded), typeof(RoadNodeTypeWasChanged), typeof(RoadNodeWasModified),
            typeof(RoadNodeWasMigrated), typeof(RoadNodeWasRemoved), typeof(RoadNodeWasRemovedBecauseOfMigration),
            // RoadSegment V1
            typeof(RoadSegmentV1.ImportedRoadSegment), typeof(RoadSegmentV1.OutlinedRoadSegmentRemoved),
            typeof(RoadSegmentV1.RoadSegmentAdded), typeof(RoadSegmentV1.RoadSegmentAddedToEuropeanRoad),
            typeof(RoadSegmentV1.RoadSegmentAddedToNationalRoad), typeof(RoadSegmentV1.RoadSegmentAddedToNumberedRoad),
            typeof(RoadSegmentV1.RoadSegmentAttributesModified), typeof(RoadSegmentV1.RoadSegmentGeometryModified),
            typeof(RoadSegmentV1.RoadSegmentModified), typeof(RoadSegmentV1.RoadSegmentRemoved),
            typeof(RoadSegmentV1.RoadSegmentRemovedFromEuropeanRoad), typeof(RoadSegmentV1.RoadSegmentRemovedFromNationalRoad),
            typeof(RoadSegmentV1.RoadSegmentRemovedFromNumberedRoad), typeof(RoadSegmentV1.RoadSegmentStreetNamesChanged),
            // RoadSegment V2
            typeof(RoadSegmentV2.OutlinedRoadSegmentWasAdded), typeof(RoadSegmentV2.RoadSegmentGeometryWasModified),
            typeof(RoadSegmentV2.RoadSegmentStreetNameIdWasChanged), typeof(RoadSegmentV2.RoadSegmentWasAdded),
            typeof(RoadSegmentV2.RoadSegmentWasAddedToEuropeanRoad), typeof(RoadSegmentV2.RoadSegmentWasAddedToNationalRoad),
            typeof(RoadSegmentV2.RoadSegmentWasMerged), typeof(RoadSegmentV2.RoadSegmentWasMigrated),
            typeof(RoadSegmentV2.RoadSegmentWasModified), typeof(RoadSegmentV2.RoadSegmentWasRemoved),
            typeof(RoadSegmentV2.RoadSegmentWasRemovedBecauseOfMigration), typeof(RoadSegmentV2.RoadSegmentWasRemovedFromEuropeanRoad),
            typeof(RoadSegmentV2.RoadSegmentWasRemovedFromNationalRoad), typeof(RoadSegmentV2.RoadSegmentWasRetired),
            typeof(RoadSegmentV2.RoadSegmentWasRetiredBecauseOfMerger), typeof(RoadSegmentV2.RoadSegmentWasRetiredBecauseOfSplit),
            typeof(RoadSegmentV2.RoadSegmentWasSplit),
            // GradeSeparatedJunction V1
            typeof(ImportedGradeSeparatedJunction), typeof(GradeSeparatedJunctionAdded),
            typeof(GradeSeparatedJunctionModified), typeof(GradeSeparatedJunctionRemoved),
            typeof(GradeSeparatedJunctionGeometryModified),
            // GradeSeparatedJunction V2
            typeof(GradeSeparatedJunctionWasAdded), typeof(GradeSeparatedJunctionWasModified), typeof(GradeSeparatedJunctionGeometryWasChanged),
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

        WmsWfsV2ProjectionEventCoverage.AssertHandledExactlyOnce(new GradeJunctionWmsWfsV2Projection(null!), excludeEventTypes);
    }

    // The junction point now arrives on the junction's own events; build it from a plain Lambert08 point.
    private static JunctionGeometry JunctionPoint((double X, double Y) p) =>
        JunctionGeometry.Create(new NetTopologySuite.Geometries.Point(p.X, p.Y) { SRID = WellknownSrids.Lambert08 });

    [Fact]
    public async Task WhenGradeJunctionWasAdded_ThenStoredWithGeometryFromEvent()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(1),
            RoadSegmentId2 = new RoadSegmentId(2),
            Geometry = JunctionPoint((50, 50)),
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
    public async Task WhenGradeJunctionGeometryWasChanged_ThenGeometryUpdated()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(1),
            RoadSegmentId2 = new RoadSegmentId(2),
            Geometry = JunctionPoint((50, 50)),
            Provenance = Provenance
        });
        await scenario.GivenAsync(new GradeJunctionGeometryWasChanged
        {
            GradeJunctionId = new GradeJunctionId(1),
            Geometry = JunctionPoint((25, 25)),
            Provenance = Provenance
        });

        var junction = await scenario.Find<GradeJunctionRecord>(1);
        Assert.NotNull(junction);
        Assert.NotNull(junction!.GEOMETRIE);
        Assert.Equal(25.0, junction.GEOMETRIE.Coordinate.X, 3);
        Assert.Equal(25.0, junction.GEOMETRIE.Coordinate.Y, 3);
    }

    [Fact]
    public async Task WhenGradeJunctionWasRemoved_ThenDeleted()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(1),
            RoadSegmentId2 = new RoadSegmentId(2),
            Geometry = JunctionPoint((50, 50)),
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
