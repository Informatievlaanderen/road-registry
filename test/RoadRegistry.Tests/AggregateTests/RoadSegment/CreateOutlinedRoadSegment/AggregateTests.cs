namespace RoadRegistry.Tests.AggregateTests.RoadSegment.CreateOutlinedRoadSegment;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.Tests.AggregateTests;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests
{
    private readonly IFixture _fixture = new RoadNetworkTestDataV2().Fixture;

    [Fact]
    public void StateCheck()
    {
        var evt = _fixture.Create<OutlinedRoadSegmentWasAdded>();

        var segment = RoadSegment.Create(evt);

        segment.RoadSegmentId.Should().Be(evt.RoadSegmentId);
        segment.Geometry.Should().Be(evt.Geometry);
        segment.Status.Should().Be(evt.Status);
        segment.StartNodeId.Should().BeNull();
        segment.EndNodeId.Should().BeNull();
        segment.Attributes!.GeometryDrawMethod.Should().Be(RoadSegmentGeometryDrawMethodV2.Ingeschetst);
        segment.Attributes!.AccessRestriction.Should().Be(evt.AccessRestriction);
        segment.Attributes!.Category.Should().Be(evt.Category);
        segment.Attributes!.Morphology.Should().Be(evt.Morphology);
        segment.Attributes!.StreetNameId.Should().Be(evt.StreetNameId);
        segment.Attributes!.MaintenanceAuthorityId.Should().Be(evt.MaintenanceAuthorityId);
        segment.Attributes!.SurfaceType.Should().Be(evt.SurfaceType);
        segment.Attributes!.CarTrafficDirection.Should().Be(evt.CarTrafficDirection);
        segment.Attributes!.BikeTrafficDirection.Should().Be(evt.BikeTrafficDirection);
        segment.Attributes!.PedestrianTrafficDirection.Should().Be(evt.PedestrianTrafficDirection);
        segment.Attributes!.EuropeanRoadNumbers.Should().BeEmpty();
        segment.Attributes!.NationalRoadNumbers.Should().BeEmpty();
    }
}
