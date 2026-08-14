namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ChangeRoadSegmentGeometryDrawMethod;

using System.Linq;
using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.ValueObjects;
using RoadNode = RoadRegistry.RoadNode.RoadNode;
using RoadSegment = RoadRegistry.RoadSegment.RoadSegment;

public class AggregateTests : AggregateTestBase
{
    private ScopedRoadNetwork BuildNetworkWith(RoadSegmentStatusV2 status)
    {
        var segment = RoadSegment.Create(TestData.Segment1Added with { Status = status }).WithoutChanges();

        return new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            [RoadNode.Create(TestData.Segment1StartNodeAdded), RoadNode.Create(TestData.Segment1EndNodeAdded)],
            [segment],
            [],
            []);
    }

    private ChangeRoadSegmentGeometryDrawMethodChange BuildChange(RoadSegmentGeometryDrawMethodV2 geometryDrawMethod, RoadSegmentId? roadSegmentId = null)
    {
        return new ChangeRoadSegmentGeometryDrawMethodChange
        {
            RoadSegmentId = roadSegmentId ?? TestData.Segment1Added.RoadSegmentId,
            GeometryDrawMethod = geometryDrawMethod
        };
    }

    private RoadSegmentGeometryDrawMethodV2 CurrentDrawMethodOf(ScopedRoadNetwork roadNetwork, RoadSegmentId roadSegmentId)
    {
        return roadNetwork.RoadSegments[roadSegmentId].Attributes!.GeometryDrawMethod;
    }

    private RoadSegmentGeometryDrawMethodV2 OtherDrawMethodThan(ScopedRoadNetwork roadNetwork)
    {
        var current = CurrentDrawMethodOf(roadNetwork, TestData.Segment1Added.RoadSegmentId);
        return RoadSegmentGeometryDrawMethodV2.All.First(x => x != current);
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.Gepland))]
    [InlineData(nameof(RoadSegmentStatusV2.Gerealiseerd))]
    [InlineData(nameof(RoadSegmentStatusV2.BuitenGebruik))]
    public void WhenStatusIsEditable_ThenGeometryDrawMethodIsChanged(string statusName)
    {
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.Parse(statusName));
        var geometryDrawMethod = OtherDrawMethodThan(roadNetwork);

        var result = roadNetwork.ChangeRoadSegmentGeometryDrawMethod([BuildChange(geometryDrawMethod)], TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadSegments.Modified.Should().ContainSingle();
        CurrentDrawMethodOf(roadNetwork, TestData.Segment1Added.RoadSegmentId).Should().Be(geometryDrawMethod);
    }

    [Fact]
    public void WhenGeometryDrawMethodIsChanged_ThenTheEventCarriesOnlyTheDrawMethod()
    {
        // The change rides the generic road segment modification, so everything it does not name has to stay null:
        // the event says what changed, not what the segment looks like.
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.Gerealiseerd);
        var geometryDrawMethod = OtherDrawMethodThan(roadNetwork);

        var result = roadNetwork.ChangeRoadSegmentGeometryDrawMethod([BuildChange(geometryDrawMethod)], TestData.Provenance);

        result.Problems.Should().BeEmpty();
        var @event = roadNetwork.RoadSegments[TestData.Segment1Added.RoadSegmentId].GetChanges()
            .OfType<RoadSegmentWasModified>().Should().ContainSingle().Which;
        @event.GeometryDrawMethod.Should().Be(geometryDrawMethod);
        @event.Geometry.Should().BeNull();
        @event.Status.Should().BeNull();
        @event.Morphology.Should().BeNull();
        @event.SurfaceType.Should().BeNull();
        @event.AccessRestriction.Should().BeNull();
        @event.Category.Should().BeNull();
        @event.StreetNameId.Should().BeNull();
        @event.MaintenanceAuthorityId.Should().BeNull();
        @event.CarTrafficDirection.Should().BeNull();
        @event.BikeTrafficDirection.Should().BeNull();
        @event.PedestrianTrafficDirection.Should().BeNull();
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.NietGerealiseerd))]
    [InlineData(nameof(RoadSegmentStatusV2.Gehistoreerd))]
    public void WhenStatusIsNotEditable_ThenError(string statusName)
    {
        // VAL-6: the geometry draw method may only be changed on a 'gepland', 'gerealiseerd' or 'buiten gebruik' segment.
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.Parse(statusName));

        var result = roadNetwork.ChangeRoadSegmentGeometryDrawMethod([BuildChange(OtherDrawMethodThan(roadNetwork))], TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentChangeGeometryDrawMethodStatusNotValid");
        roadNetwork.RoadSegments[TestData.Segment1Added.RoadSegmentId].GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenSegmentDoesNotExist_ThenNotFound()
    {
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.Gerealiseerd);

        var result = roadNetwork.ChangeRoadSegmentGeometryDrawMethod(
            [BuildChange(RoadSegmentGeometryDrawMethodV2.Ingemeten, new RoadSegmentId(999))], TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentNotFound");
    }

    [Fact]
    public void WhenSegmentDidNotCompleteInwinning_ThenError()
    {
        // A non-migrated (V1) segment carries no attributes at all, so it must be rejected up front instead of
        // failing while the change is applied.
        var notMigrated = RoadSegment.CreateForMigration(
            TestData.Segment1Added.RoadSegmentId,
            TestData.Segment1Added.Geometry,
            RoadSegmentStatusV2.Gerealiseerd,
            TestData.Segment1Added.StartNodeId,
            TestData.Segment1Added.EndNodeId);

        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            [RoadNode.Create(TestData.Segment1StartNodeAdded), RoadNode.Create(TestData.Segment1EndNodeAdded)],
            [notMigrated],
            [],
            []);

        var result = roadNetwork.ChangeRoadSegmentGeometryDrawMethod(
            [BuildChange(RoadSegmentGeometryDrawMethodV2.Ingemeten)], TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentNotCompletedInwinning");
        notMigrated.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenTheValueIsAlreadyTheCurrentOne_ThenNoChangeSummaryIsEmitted()
    {
        // Setting the draw method to the value it already has is accepted, but records nothing on the segment.
        // Emitting a road network change summary for that would put an event in the stream saying nothing happened.
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.Gerealiseerd);
        var current = CurrentDrawMethodOf(roadNetwork, TestData.Segment1Added.RoadSegmentId);

        var result = roadNetwork.ChangeRoadSegmentGeometryDrawMethod([BuildChange(current)], TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.HasChanges().Should().BeFalse();
        result.Summary.RoadSegments.Modified.Should().BeEmpty();
        roadNetwork.GetChanges().Should().BeEmpty("no road network change event may be raised when nothing moved");
    }

    [Fact]
    public void WhenOnlyOneOfSeveralSegmentsActuallyChanges_ThenTheSummaryIsStillEmitted()
    {
        // The guard is on "did anything move", not "was every change effective": one real change alongside a no-op on
        // another segment still has to produce the summary.
        var roadNetwork = new ScopedRoadNetwork(Fixture.Create<ScopedRoadNetworkId>(),
            [
                RoadNode.Create(TestData.Segment1StartNodeAdded), RoadNode.Create(TestData.Segment1EndNodeAdded),
                RoadNode.Create(TestData.Segment2StartNodeAdded), RoadNode.Create(TestData.Segment2EndNodeAdded)
            ],
            [
                RoadSegment.Create(TestData.Segment1Added).WithoutChanges(),
                RoadSegment.Create(TestData.Segment2Added).WithoutChanges()
            ],
            [],
            []);

        var noOp = BuildChange(
            CurrentDrawMethodOf(roadNetwork, TestData.Segment1Added.RoadSegmentId),
            TestData.Segment1Added.RoadSegmentId);

        var current2 = CurrentDrawMethodOf(roadNetwork, TestData.Segment2Added.RoadSegmentId);
        var real = BuildChange(
            RoadSegmentGeometryDrawMethodV2.All.First(x => x != current2),
            TestData.Segment2Added.RoadSegmentId);

        var result = roadNetwork.ChangeRoadSegmentGeometryDrawMethod([noOp, real], TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadSegments.Modified.Should().ContainSingle()
            .Which.Should().Be(TestData.Segment2Added.RoadSegmentId);
        roadNetwork.GetChanges().Should().NotBeEmpty();
    }

    [Fact]
    public void WhenOneOfTheSegmentsIsNotEditable_ThenNothingIsApplied()
    {
        // The whole request is rejected up front, so it can never land half-applied.
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.Gehistoreerd);

        var result = roadNetwork.ChangeRoadSegmentGeometryDrawMethod([BuildChange(OtherDrawMethodThan(roadNetwork))], TestData.Provenance);

        result.Problems.Should().NotBeEmpty();
        result.Summary.RoadSegments.Modified.Should().BeEmpty();
        roadNetwork.GetChanges().Should().BeEmpty();
    }
}
