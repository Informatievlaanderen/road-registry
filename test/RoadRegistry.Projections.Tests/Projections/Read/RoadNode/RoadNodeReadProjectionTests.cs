namespace RoadRegistry.Projections.Tests.Projections.ReadProjections.RoadNode;

using System;
using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using NetTopologySuite.Geometries;
using RoadRegistry.Read.Projections;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadNode.Events.V1.ValueObjects;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.ValueObjects;
using RoadNodeV1 = RoadRegistry.RoadNode.Events.V1;

public class RoadNodeReadProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ReadProjectionScenario Scenario() => new(new RoadNodeReadProjection());
    private ProvenanceData Provenance => new(_testData.Provenance);

    [Fact]
    public async Task WhenRoadNodeWasAdded_ThenStored()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded);

        var node = await scenario.Load<RoadNodeReadItem>(1);
        Assert.NotNull(node);
        Assert.Equal(new RoadNodeId(1), node.RoadNodeId);
        Assert.True(node.IsV2);
        Assert.False(node.IsRemoved);
        Assert.Empty(node.RoadSegmentIds);
        Assert.Null(node.Type);
    }

    [Fact]
    public async Task WhenImportedRoadNodeWasAdded_ThenLambert72GeometryRoundedToCm()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new RoadNodeV1.ImportedRoadNode
        {
            RoadNodeId = 7,
            Geometry = RoadNodeGeometry.Create(new Point(155000.1234, 195000.5678) { SRID = WellknownSrids.Lambert72 }),
            Type = "RealNode",
            Version = 1,
            Origin = new ImportedOriginProperties
            {
                Application = "unit-test",
                Operator = "unit-test",
                Organization = "unit-test",
                OrganizationId = "unit-test",
                Since = DateTime.UtcNow,
                TransactionId = 1
            },
            When = DateTimeOffset.UtcNow,
            Provenance = Provenance
        });

        var node = await scenario.Load<RoadNodeReadItem>(7);
        Assert.NotNull(node);
        Assert.Equal(155000.12, node!.Geometry.Lambert72.Value.X);
        Assert.Equal(195000.57, node.Geometry.Lambert72.Value.Y);
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

        var node = await scenario.Load<RoadNodeReadItem>(1);
        Assert.Equal(type.ToString(), node!.Type);
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

        var node = await scenario.Load<RoadNodeReadItem>(1);
        Assert.True(node!.Grensknoop);
    }

    [Fact]
    public async Task WhenRoadNodeWasRemoved_ThenMarkedRemoved()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(_testData.Segment1StartNodeAdded);
        await scenario.GivenAsync(new RoadNodeWasRemoved
        {
            RoadNodeId = new RoadNodeId(1),
            Provenance = Provenance
        });

        var node = await scenario.Load<RoadNodeReadItem>(1);
        Assert.True(node!.IsRemoved);
    }

    [Fact]
    public async Task WhenRemovingUnknownRoadNode_ThenThrows()
    {
        var scenario = Scenario();

        await Assert.ThrowsAsync<InvalidOperationException>(() => scenario.GivenAsync(new RoadNodeWasRemoved
        {
            RoadNodeId = new RoadNodeId(999),
            Provenance = Provenance
        }));
    }
}
