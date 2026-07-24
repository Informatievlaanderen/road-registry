namespace RoadRegistry.Projections.Tests.Projections.WmsWfsV2.GradeSeparatedJunction;

using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using Be.Vlaanderen.Basisregisters.Shaperon;
using NetTopologySuite.Geometries;
using RoadRegistry.GradeJunction.Events.V2;
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
using GradeSeparatedJunctionV1 = RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadSegmentV1 = RoadRegistry.RoadSegment.Events.V1;
using RoadSegmentV2 = RoadRegistry.RoadSegment.Events.V2;

public class GradeSeparatedJunctionWmsWfsV2ProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private WmsWfsV2ProjectionScenario Scenario() =>
        new(new GradeSeparatedJunctionWmsWfsV2Projection());

    private ProvenanceData Provenance => new(_testData.Provenance);

    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        // This projection handles all GradeSeparatedJunction V1 + V2 events (including geometry-changed); the junction
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
            // GradeJunction V2
            typeof(GradeJunctionWasAdded), typeof(GradeJunctionWasModified), typeof(GradeJunctionGeometryWasChanged), typeof(GradeJunctionWasRemoved),
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

        WmsWfsV2ProjectionEventCoverage.AssertHandledExactlyOnce(new GradeSeparatedJunctionWmsWfsV2Projection(), excludeEventTypes);
    }

    // The junction point now arrives on the junction's own events; build it from a plain Lambert08 point.
    private static JunctionGeometry JunctionPoint((double X, double Y) p) =>
        JunctionGeometry.Create(new NetTopologySuite.Geometries.Point(p.X, p.Y) { SRID = WellknownSrids.Lambert08 });

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasAdded_ThenStoredWithTypeAndGeometryFromEvent()
    {
        var scenario = Scenario();
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(2),
            Type = type,
            Geometry = JunctionPoint((50, 50)),
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
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();
        var newType = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(2),
            Type = type,
            Geometry = JunctionPoint((50, 50)),
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
    public async Task WhenGradeSeparatedJunctionGeometryWasChanged_ThenGeometryUpdated()
    {
        var scenario = Scenario();
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(2),
            Type = type,
            Geometry = JunctionPoint((50, 50)),
            Provenance = Provenance
        });
        await scenario.GivenAsync(new GradeSeparatedJunctionGeometryWasChanged
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            Geometry = JunctionPoint((25, 25)),
            Provenance = Provenance
        });

        var junction = await scenario.Find<GradeSeparatedJunctionRecord>(1);
        Assert.NotNull(junction);
        Assert.NotNull(junction!.GEOMETRIE);
        Assert.Equal(25.0, junction.GEOMETRIE.Coordinate.X, 3);
        Assert.Equal(25.0, junction.GEOMETRIE.Coordinate.Y, 3);
    }

    [Fact]
    public async Task WhenV1GradeSeparatedJunctionGeometryModified_ThenGeometryUpdatedAndNormalizedToLambert08()
    {
        var scenario = Scenario();
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(2),
            Type = type,
            Geometry = JunctionPoint((50, 50)),
            Provenance = Provenance
        });

        // The V1 (legacy) geometry-changed event carries the junction point in Lambert72; PBS must normalize it to Lambert08.
        var lambert72Point = JunctionGeometry.Create(new NetTopologySuite.Geometries.Point(155000, 195000) { SRID = WellknownSrids.Lambert72 });
        await scenario.GivenAsync(new GradeSeparatedJunctionV1.GradeSeparatedJunctionGeometryModified
        {
            Id = 1,
            Geometry = lambert72Point,
            Provenance = Provenance
        });

        var junction = await scenario.Find<GradeSeparatedJunctionRecord>(1);
        Assert.NotNull(junction);
        Assert.NotNull(junction!.GEOMETRIE);
        Assert.Equal(WellknownSrids.Lambert08, junction.GEOMETRIE!.SRID);
        // Geometry no longer matches the original Lambert08 (50, 50) point; it was replaced by the transformed Lambert72 point.
        Assert.NotEqual(50.0, junction.GEOMETRIE.Coordinate.X, 3);
    }

    [Fact]
    public async Task WhenV1GradeSeparatedJunctionAdded_ThenStoredWithGeometryFromEvent()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new GradeSeparatedJunctionV1.GradeSeparatedJunctionAdded
        {
            Id = 1,
            LowerRoadSegmentId = 1,
            UpperRoadSegmentId = 2,
            TemporaryId = 1,
            Type = "Tunnel",
            Geometry = JunctionPoint((50, 50)),
            Provenance = Provenance
        });

        var junction = await scenario.Find<GradeSeparatedJunctionRecord>(1);
        Assert.NotNull(junction);
        Assert.Equal(1, junction!.ON_WS_OIDN);
        Assert.Equal(2, junction.BO_WS_OIDN);
        Assert.NotNull(junction.GEOMETRIE);
        Assert.Equal(50.0, junction.GEOMETRIE.Coordinate.X, 3);
        Assert.Equal(50.0, junction.GEOMETRIE.Coordinate.Y, 3);
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasRemoved_ThenDeleted()
    {
        var scenario = Scenario();
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(1),
            UpperRoadSegmentId = new RoadSegmentId(2),
            Type = type,
            Geometry = JunctionPoint((50, 50)),
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
