namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.Tests.AggregateTests;

public class GradeJunctionReadProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ReadProjectionScenario Scenario() => new(new GradeJunctionReadProjection());
    private ProvenanceData Provenance => new(_testData.Provenance);

    [Fact]
    public async Task WhenGradeJunctionWasAdded_ThenStored()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(10),
            RoadSegmentId2 = new RoadSegmentId(20),
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeJunctionReadItem>(1);
        Assert.NotNull(junction);
        Assert.Equal(new GradeJunctionId(1), junction!.GradeJunctionId);
        Assert.Equal(new RoadSegmentId(10), junction.RoadSegmentId1);
        Assert.Equal(new RoadSegmentId(20), junction.RoadSegmentId2);
        Assert.True(junction.IsV2);
        Assert.False(junction.IsRemoved);
    }

    [Fact]
    public async Task WhenGradeJunctionWasRemoved_ThenMarkedRemoved()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new GradeJunctionWasAdded
        {
            GradeJunctionId = new GradeJunctionId(1),
            RoadSegmentId1 = new RoadSegmentId(10),
            RoadSegmentId2 = new RoadSegmentId(20),
            Provenance = Provenance
        });
        await scenario.GivenAsync(new GradeJunctionWasRemoved
        {
            GradeJunctionId = new GradeJunctionId(1),
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeJunctionReadItem>(1);
        Assert.True(junction!.IsRemoved);
    }

    [Fact]
    public async Task WhenRemovingUnknownGradeJunction_ThenThrows()
    {
        var scenario = Scenario();

        await Assert.ThrowsAsync<InvalidOperationException>(() => scenario.GivenAsync(new GradeJunctionWasRemoved
        {
            GradeJunctionId = new GradeJunctionId(999),
            Provenance = Provenance
        }));
    }
}
