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
