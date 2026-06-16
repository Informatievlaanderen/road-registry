namespace RoadRegistry.Projections.Tests.Projections.ReadProjections;

using System;
using System.Threading.Tasks;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Read.Projections;
using RoadRegistry.Tests.AggregateTests;
using V1 = RoadRegistry.GradeSeparatedJunction.Events.V1;

public class GradeSeparatedJunctionReadProjectionTests
{
    private readonly RoadNetworkTestDataV2 _testData = new();

    private ReadProjectionScenario Scenario() => new(new GradeSeparatedJunctionReadProjection());
    private ProvenanceData Provenance => new(_testData.Provenance);

    [Fact]
    public async Task WhenV1GradeSeparatedJunctionAdded_ThenStored()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new V1.GradeSeparatedJunctionAdded
        {
            Id = 1,
            TemporaryId = -1,
            LowerRoadSegmentId = 10,
            UpperRoadSegmentId = 20,
            Type = "GelijkgrondseKruising",
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeSeparatedJunctionReadItem>(1);
        Assert.NotNull(junction);
        Assert.Equal(new RoadSegmentId(10), junction!.LowerRoadSegmentId);
        Assert.Equal(new RoadSegmentId(20), junction.UpperRoadSegmentId);
        Assert.Equal("GelijkgrondseKruising", junction.Type);
        Assert.False(junction.IsV2);
    }

    [Fact]
    public async Task WhenV1GradeSeparatedJunctionModified_ThenUpdated()
    {
        var scenario = Scenario();

        await scenario.GivenAsync(new V1.GradeSeparatedJunctionAdded
        {
            Id = 1,
            TemporaryId = -1,
            LowerRoadSegmentId = 10,
            UpperRoadSegmentId = 20,
            Type = "GelijkgrondseKruising",
            Provenance = Provenance
        });
        await scenario.GivenAsync(new V1.GradeSeparatedJunctionModified
        {
            Id = 1,
            LowerRoadSegmentId = 11,
            UpperRoadSegmentId = 21,
            Type = "Tunnel",
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeSeparatedJunctionReadItem>(1);
        Assert.Equal(new RoadSegmentId(11), junction!.LowerRoadSegmentId);
        Assert.Equal(new RoadSegmentId(21), junction.UpperRoadSegmentId);
        Assert.Equal("Tunnel", junction.Type);
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasAdded_ThenStored()
    {
        var scenario = Scenario();
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(10),
            UpperRoadSegmentId = new RoadSegmentId(20),
            Type = type,
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeSeparatedJunctionReadItem>(1);
        Assert.NotNull(junction);
        Assert.Equal(type.ToString(), junction!.Type);
        Assert.True(junction.IsV2);
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasModified_ThenNullFieldsKeepExistingValues()
    {
        var scenario = Scenario();
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(10),
            UpperRoadSegmentId = new RoadSegmentId(20),
            Type = type,
            Provenance = Provenance
        });
        await scenario.GivenAsync(new GradeSeparatedJunctionWasModified
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(99),
            UpperRoadSegmentId = null,
            Type = null,
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeSeparatedJunctionReadItem>(1);
        Assert.Equal(new RoadSegmentId(99), junction!.LowerRoadSegmentId);
        Assert.Equal(new RoadSegmentId(20), junction.UpperRoadSegmentId);
        Assert.Equal(type.ToString(), junction.Type);
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasRemoved_ThenMarkedRemoved()
    {
        var scenario = Scenario();
        var type = _testData.Fixture.Create<GradeSeparatedJunctionTypeV2>();

        await scenario.GivenAsync(new GradeSeparatedJunctionWasAdded
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            LowerRoadSegmentId = new RoadSegmentId(10),
            UpperRoadSegmentId = new RoadSegmentId(20),
            Type = type,
            Provenance = Provenance
        });
        await scenario.GivenAsync(new GradeSeparatedJunctionWasRemoved
        {
            GradeSeparatedJunctionId = new GradeSeparatedJunctionId(1),
            Provenance = Provenance
        });

        var junction = await scenario.Load<GradeSeparatedJunctionReadItem>(1);
        Assert.True(junction!.IsRemoved);
    }
}
