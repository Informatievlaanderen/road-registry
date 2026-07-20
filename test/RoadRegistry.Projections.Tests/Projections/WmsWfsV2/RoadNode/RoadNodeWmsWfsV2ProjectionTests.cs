namespace RoadRegistry.Projections.Tests.Projections.WmsWfsV2.RoadNode;

using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.WmsWfsV2.Projections;
using RoadRegistry.WmsWfsV2.Schema.Records;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V1;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.ScopedRoadNetwork.Events.V1;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.StreetName.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.ValueObjects;
using RoadNodeV1 = RoadRegistry.RoadNode.Events.V1;

public class RoadNodeWmsWfsV2ProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private WmsWfsV2ProjectionScenario Scenario() =>
        new(new RoadNodeWmsWfsV2Projection());

    private ProvenanceData Provenance => new(_testData.Provenance);

    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        // This projection handles all RoadNode V1 + V2 events; everything else is excluded.
        var excludeEventTypes = new[]
        {
            // RoadSegment V1
            typeof(ImportedRoadSegment), typeof(OutlinedRoadSegmentRemoved), typeof(RoadSegmentAdded),
            typeof(RoadSegmentAddedToEuropeanRoad), typeof(RoadSegmentAddedToNationalRoad), typeof(RoadSegmentAddedToNumberedRoad),
            typeof(RoadSegmentAttributesModified), typeof(RoadSegmentGeometryModified), typeof(RoadSegmentModified),
            typeof(RoadSegmentRemoved), typeof(RoadSegmentRemovedFromEuropeanRoad), typeof(RoadSegmentRemovedFromNationalRoad),
            typeof(RoadSegmentRemovedFromNumberedRoad), typeof(RoadSegmentStreetNamesChanged),
            // RoadSegment V2
            typeof(OutlinedRoadSegmentWasAdded), typeof(RoadSegmentGeometryWasModified), typeof(RoadSegmentStreetNameIdWasChanged),
            typeof(RoadSegmentWasAdded), typeof(RoadSegmentWasAddedToEuropeanRoad), typeof(RoadSegmentWasAddedToNationalRoad),
            typeof(RoadSegmentWasMerged), typeof(RoadSegmentWasMigrated), typeof(RoadSegmentWasModified),
            typeof(RoadSegmentWasRemoved), typeof(RoadSegmentWasRemovedBecauseOfMigration), typeof(RoadSegmentWasRemovedFromEuropeanRoad),
            typeof(RoadSegmentWasRemovedFromNationalRoad), typeof(RoadSegmentWasRetired), typeof(RoadSegmentWasRetiredBecauseOfMerger),
            typeof(RoadSegmentWasRetiredBecauseOfSplit), typeof(RoadSegmentWasSplit),
            // GradeJunction V2
            typeof(GradeJunctionWasAdded), typeof(GradeJunctionGeometryWasChanged), typeof(GradeJunctionWasRemoved),
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

        WmsWfsV2ProjectionEventCoverage.AssertHandledExactlyOnce(new RoadNodeWmsWfsV2Projection(), excludeEventTypes);
    }

    [Fact]
    public async Task WhenRoadNodeWasAdded_ThenStored()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded);

        var node = await scenario.Find<RoadNodeRecord>(1);
        Assert.NotNull(node);
        Assert.Equal(1, node!.WK_OIDN);
        Assert.Equal(0, node.GRENSKNOOP); // Grensknoop == false
        Assert.Null(node.TYPE); // a freshly added V2 node has no type until RoadNodeTypeWasChanged
        Assert.NotNull(node.GEOMETRIE);
        Assert.NotEqual(default, node.CREATIE);
        Assert.NotEqual(default, node.VERSIE);
    }

    [Fact]
    public async Task WhenRoadNodeTypeWasChanged_ThenTypeUpdated()
    {
        var scenario = Scenario();
        var type = _testData.Fixture.Create<RoadNodeTypeV2>();

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded);
        await scenario.GivenAsync(new RoadNodeTypeWasChanged
        {
            RoadNodeId = new RoadNodeId(1),
            Type = type,
            Provenance = Provenance
        });

        var node = await scenario.Find<RoadNodeRecord>(1);
        Assert.Equal(type.Translation.Identifier, node!.TYPE);
        Assert.Equal(type.Translation.Name, node.LBLTYPE);
    }

    [Fact]
    public async Task WhenRoadNodeWasModified_ThenGrensknoopUpdated()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded);
        await scenario.GivenAsync(new RoadNodeWasModified
        {
            RoadNodeId = new RoadNodeId(1),
            Geometry = null,
            Grensknoop = true,
            Provenance = Provenance
        });

        var node = await scenario.Find<RoadNodeRecord>(1);
        Assert.Equal(1, node!.GRENSKNOOP);
    }

    [Fact]
    public async Task WhenRoadNodeWasMigrated_ThenStored()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new RoadNodeWasMigrated
        {
            RoadNodeId = new RoadNodeId(42),
            Geometry = _testData.Segment1StartNodeAdded.Geometry,
            Grensknoop = true,
            Provenance = Provenance
        });

        var node = await scenario.Find<RoadNodeRecord>(42);
        Assert.NotNull(node);
        Assert.Equal(1, node!.GRENSKNOOP);
        Assert.NotNull(node.GEOMETRIE);
    }

    [Fact]
    public async Task WhenRoadNodeWasRemoved_ThenDeleted()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded);
        await scenario.GivenAsync(new RoadNodeWasRemoved
        {
            RoadNodeId = new RoadNodeId(1),
            Provenance = Provenance
        });

        Assert.Null(await scenario.Find<RoadNodeRecord>(1));
    }

    [Fact]
    public async Task WhenRemovingUnknownRoadNode_ThenNoOp()
    {
        // Unlike the read projection (which throws), the PBS handlers are null-safe so daemon replays stay idempotent.
        var scenario = Scenario();

        await scenario.GivenAsync(new RoadNodeWasRemoved
        {
            RoadNodeId = new RoadNodeId(999),
            Provenance = Provenance
        });

        Assert.Null(await scenario.Find<RoadNodeRecord>(999));
    }

    [Fact]
    public async Task WhenV1RoadNodeAdded_ThenStoredWithMappedTypeAndNoGrensknoop()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new RoadNodeV1.RoadNodeAdded
        {
            RoadNodeId = 7,
            Geometry = _testData.Segment1StartNodeAdded.Geometry,
            Type = nameof(RoadNodeType.RealNode), // V1 identifier 1 -> V2 identifier 10
            Provenance = Provenance
        });

        var node = await scenario.Find<RoadNodeRecord>(7);
        Assert.NotNull(node);
        Assert.Null(node!.GRENSKNOOP); // V1 nodes carry no grensknoop
        Assert.Equal(10, node.TYPE);
        Assert.NotNull(node.GEOMETRIE);
    }
}
