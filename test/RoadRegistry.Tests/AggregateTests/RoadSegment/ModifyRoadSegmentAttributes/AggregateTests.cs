namespace RoadRegistry.Tests.AggregateTests.RoadSegment.ModifyRoadSegmentAttributes;

using System.Linq;
using AutoFixture;
using FluentAssertions;
using RoadRegistry.RoadSegment.Changes;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
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

    // Only the morphology is changed; geometry, draw method and status stay null so they are left untouched.
    private ModifyRoadSegmentChange BuildAttributesChange(ScopedRoadNetwork roadNetwork, RoadSegmentMorphologyV2 morphology)
    {
        var roadSegmentId = TestData.Segment1Added.RoadSegmentId;
        var length = roadNetwork.RoadSegments[roadSegmentId].Geometry.Value.Length;

        var values = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>();
        values.Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(length), morphology);

        return new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(roadSegmentId),
            Morphology = values
        };
    }

    private RoadSegmentMorphologyV2 OtherMorphologyThan(ScopedRoadNetwork roadNetwork)
    {
        var current = roadNetwork.RoadSegments[TestData.Segment1Added.RoadSegmentId].Attributes!.Morphology.Values.First().Value;
        return RoadSegmentMorphologyV2.All.First(x => x != current);
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.Gepland))]
    [InlineData(nameof(RoadSegmentStatusV2.Gerealiseerd))]
    [InlineData(nameof(RoadSegmentStatusV2.BuitenGebruik))]
    public void WhenStatusIsEditable_ThenAttributesAreModified(string statusName)
    {
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.Parse(statusName));
        var change = BuildAttributesChange(roadNetwork, OtherMorphologyThan(roadNetwork));

        var result = roadNetwork.ModifyRoadSegmentAttributes([change], TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadSegments.Modified.Should().ContainSingle();
        roadNetwork.RoadSegments[TestData.Segment1Added.RoadSegmentId].GetChanges()
            .OfType<RoadSegmentWasModified>().Should().ContainSingle();
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.NietGerealiseerd))]
    [InlineData(nameof(RoadSegmentStatusV2.Gehistoreerd))]
    public void WhenStatusIsNotEditable_ThenError(string statusName)
    {
        // VAL-35: attribute values may only be changed on a 'gepland', 'gerealiseerd' or 'buiten gebruik' segment.
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.Parse(statusName));
        var change = BuildAttributesChange(roadNetwork, OtherMorphologyThan(roadNetwork));

        var result = roadNetwork.ModifyRoadSegmentAttributes([change], TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentChangeAttributesStatusNotValid");
        roadNetwork.RoadSegments[TestData.Segment1Added.RoadSegmentId].GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenSegmentDidNotCompleteInwinning_ThenError()
    {
        // A non-migrated (V1) segment has no dynamically segmented attributes at all, so it must be rejected up front
        // instead of failing while the change is applied.
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

        var values = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>();
        values.Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(notMigrated.Geometry.Value.Length), RoadSegmentMorphologyV2.All.First());
        var change = new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(TestData.Segment1Added.RoadSegmentId),
            Morphology = values
        };

        var result = roadNetwork.ModifyRoadSegmentAttributes([change], TestData.Provenance);

        result.Problems.Should().ContainSingle()
            .Which.Reason.Should().Be("RoadSegmentNotCompletedInwinning");
        notMigrated.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenTheValueIsAlreadyTheCurrentOne_ThenNoChangeSummaryIsEmitted()
    {
        // Setting an attribute to the value it already has is accepted, but records nothing on the segment. Emitting
        // a road network change summary for that would put an event in the stream saying nothing happened.
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.Gerealiseerd);
        var roadSegmentId = TestData.Segment1Added.RoadSegmentId;
        var current = roadNetwork.RoadSegments[roadSegmentId].Attributes!.Morphology.Values.First().Value;

        var result = roadNetwork.ModifyRoadSegmentAttributes([BuildAttributesChange(roadNetwork, current)], TestData.Provenance);

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

        var noOp = BuildMorphologyChange(roadNetwork, TestData.Segment1Added.RoadSegmentId,
            roadNetwork.RoadSegments[TestData.Segment1Added.RoadSegmentId].Attributes!.Morphology.Values.First().Value);

        var current2 = roadNetwork.RoadSegments[TestData.Segment2Added.RoadSegmentId].Attributes!.Morphology.Values.First().Value;
        var real = BuildMorphologyChange(roadNetwork, TestData.Segment2Added.RoadSegmentId,
            RoadSegmentMorphologyV2.All.First(x => x != current2));

        var result = roadNetwork.ModifyRoadSegmentAttributes([noOp, real], TestData.Provenance);

        result.Problems.Should().BeEmpty();
        result.Summary.RoadSegments.Modified.Should().ContainSingle()
            .Which.Should().Be(TestData.Segment2Added.RoadSegmentId);
        roadNetwork.GetChanges().Should().NotBeEmpty();
    }

    private static ModifyRoadSegmentChange BuildMorphologyChange(ScopedRoadNetwork roadNetwork, RoadSegmentId roadSegmentId, RoadSegmentMorphologyV2 morphology)
    {
        var length = roadNetwork.RoadSegments[roadSegmentId].Geometry.Value.Length;

        var values = new RoadSegmentDynamicAttributeValues<RoadSegmentMorphologyV2>();
        values.Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(length), morphology);

        return new ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(roadSegmentId),
            Morphology = values
        };
    }

    [Fact]
    public void WhenOneOfTheSegmentsIsNotEditable_ThenNothingIsApplied()
    {
        // The whole request is rejected up front, so it can never land half-applied.
        var roadNetwork = BuildNetworkWith(RoadSegmentStatusV2.Gehistoreerd);
        var change = BuildAttributesChange(roadNetwork, OtherMorphologyThan(roadNetwork));

        var result = roadNetwork.ModifyRoadSegmentAttributes([change], TestData.Provenance);

        result.Problems.Should().NotBeEmpty();
        result.Summary.RoadSegments.Modified.Should().BeEmpty();
        roadNetwork.GetChanges().Should().BeEmpty();
    }
}
