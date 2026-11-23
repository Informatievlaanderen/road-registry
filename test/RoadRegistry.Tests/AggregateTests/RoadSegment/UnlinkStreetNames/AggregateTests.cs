namespace RoadRegistry.Tests.AggregateTests.RoadSegment.UnlinkStreetNames;

using System.Linq;
using FluentAssertions;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ScopedRoadNetwork;
using RoadRegistry.ScopedRoadNetwork.ValueObjects;
using RoadRegistry.Tests.AggregateTests.Framework;
using RoadRegistry.Tests.AggregateTests.RoadSegment.ChangeRoadSegmentStatus;
using RoadRegistry.ValueObjects;

// GAWR-7784: a street name is linked to a road segment only while both are active. A road segment lets go of its
// street names when it is removed, historized or not realized - whichever way it gets there.
public class AggregateTests : StatusChangeAggregateTestBase
{
    private static readonly StreetNameLocalId LeftStreetNameId = new(501);
    private static readonly StreetNameLocalId RightStreetNameId = new(502);

    // A segment outside the network with a street name on either side, alongside a realized road it shares no node with.
    private ScopedRoadNetwork BuildUnconnectedNetworkWith(RoadSegmentStatusV2 status)
    {
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(11, 200, 0, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [westNode, eastNode],
            [
                WithStreetNames(BuildSegment(ChangedSegmentId, null, null, BuildGeometry((100, 40), (100, 120)), status)),
                BuildSegment(NeighbourSegmentId, westNode, eastNode, BuildGeometry((0, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd)
            ]);
    }

    // A realized road from (0,0) to (200,0) with a street name on either side.
    private ScopedRoadNetwork BuildRealizedNetwork()
    {
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(11, 200, 0, RoadNodeTypeV2.Eindknoop);

        return BuildNetwork(
            [westNode, eastNode],
            [WithStreetNames(BuildSegment(ChangedSegmentId, westNode, eastNode, BuildGeometry((0, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd))]);
    }

    private static RoadSegmentWasAdded WithStreetNames(RoadSegmentWasAdded segment)
    {
        var to = new RoadSegmentPositionV2(segment.Geometry.Value.Length);
        return segment with
        {
            StreetNameId = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
                .Add(RoadSegmentPositionV2.Zero, to, RoadSegmentAttributeSide.Links, LeftStreetNameId)
                .Add(RoadSegmentPositionV2.Zero, to, RoadSegmentAttributeSide.Rechts, RightStreetNameId)
        };
    }

    private static void ShouldHaveNoStreetNames(RoadRegistry.RoadSegment.RoadSegment roadSegment)
    {
        roadSegment.Attributes!.StreetNameId.Values.Should().OnlyContain(x => x.Value == StreetNameLocalId.NotApplicable);
    }

    [Theory]
    [InlineData(UnconnectedAggregateTests.OutOfUseToHistorized)]
    [InlineData(UnconnectedAggregateTests.PlannedToNotRealized)]
    public void WhenTheSegmentBecomesInactive_ThenItLetsGoOfItsStreetNamesAfterTheStatusChange(string change)
    {
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var roadNetwork = BuildUnconnectedNetworkWith(statusChange.From);
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];

        var result = roadNetwork.ChangeRoadSegmentStatus(statusChange, roadSegment.RoadSegmentId, AllGeometryDrawMethods, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        roadSegment.Status.Should().Be(statusChange.To);
        ShouldHaveNoStreetNames(roadSegment);

        var changes = roadSegment.GetChanges().ToList();
        changes[0].Should().BeOfType(statusChange.EventType);
        changes.Skip(1).Cast<RoadSegmentStreetNameIdWasChanged>()
            .Select(x => (x.OldStreetNameId, x.NewStreetNameId))
            .Should().BeEquivalentTo([(LeftStreetNameId, StreetNameLocalId.NotApplicable), (RightStreetNameId, StreetNameLocalId.NotApplicable)]);
    }

    [Theory]
    [InlineData(UnconnectedAggregateTests.HistorizedToOutOfUse)]
    [InlineData(UnconnectedAggregateTests.NotRealizedToPlanned)]
    public void WhenTheSegmentBecomesActive_ThenItsStreetNamesAreLeftAsTheyAre(string change)
    {
        // Nothing is linked again either: a street name an inactive segment still has is a leftover, not a link.
        var statusChange = RoadSegmentStatusChange.Parse(change);
        var roadNetwork = BuildUnconnectedNetworkWith(statusChange.From);
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        var streetNamesBefore = roadSegment.Attributes!.StreetNameId;

        var result = roadNetwork.ChangeRoadSegmentStatus(statusChange, roadSegment.RoadSegmentId, AllGeometryDrawMethods, IdGenerator(), TestData.Provenance);

        result.Problems.Should().BeEmpty();
        roadSegment.Attributes!.StreetNameId.Should().Be(streetNamesBefore);
        roadSegment.GetChanges().OfType<RoadSegmentStreetNameIdWasChanged>().Should().BeEmpty();
    }

    [Fact]
    public void WhenARealizedSegmentIsHistorized_ThenItLetsGoOfItsStreetNames()
    {
        // What an upload does to a road segment it leaves out.
        var roadNetwork = BuildRealizedNetwork();
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];

        var problems = roadSegment.Historize(new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance));

        problems.Should().BeEmpty();
        roadSegment.Status.Should().Be(RoadSegmentStatusV2.Gehistoreerd);
        ShouldHaveNoStreetNames(roadSegment);
        roadSegment.GetChanges().OfType<RoadSegmentStreetNameIdWasChanged>().Should().HaveCount(2);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void WhenAnUploadMakesTheSegmentInactive_ThenItEndsUpWithoutStreetNames(bool alsoChangesTheStreetNames)
    {
        // Street names the same modification hands it are let go of as well: an inactive segment has none.
        var roadNetwork = BuildUnconnectedNetworkWith(RoadSegmentStatusV2.Gepland);
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        var to = new RoadSegmentPositionV2(roadSegment.Geometry.Value.Length);
        var change = new RoadRegistry.RoadSegment.Changes.ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(roadSegment.RoadSegmentId),
            Status = RoadSegmentStatusV2.NietGerealiseerd,
            StreetNameId = alsoChangesTheStreetNames
                ? new RoadSegmentDynamicAttributeValues<StreetNameLocalId>(new StreetNameLocalId(503), roadSegment.Geometry)
                : null
        };

        var problems = roadSegment.Modify(change, new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance));

        problems.Should().HaveNoError();
        roadSegment.Status.Should().Be(RoadSegmentStatusV2.NietGerealiseerd);
        ShouldHaveNoStreetNames(roadSegment);
        roadSegment.GetChanges().Last().Should().BeOfType<RoadSegmentStreetNameIdWasChanged>();
    }

    [Fact]
    public void WhenAnInactiveSegmentIsModifiedWithoutAnyChange_ThenNothingIsRecorded()
    {
        var roadNetwork = BuildUnconnectedNetworkWith(RoadSegmentStatusV2.Gehistoreerd);
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];

        var problems = roadSegment.Modify(
            new RoadRegistry.RoadSegment.Changes.ModifyRoadSegmentChange
            {
                RoadSegmentIdReference = new RoadSegmentIdReference(roadSegment.RoadSegmentId),
                Status = RoadSegmentStatusV2.Gehistoreerd
            },
            new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance));

        problems.Should().HaveNoError();
        roadSegment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenTheSegmentIsRemoved_ThenItLetsGoOfItsStreetNamesBeforeTheRemoval()
    {
        // Before, not after: the projections drop a removed road segment.
        var roadNetwork = BuildRealizedNetwork();
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];

        var problems = roadSegment.Remove(TestData.Provenance);

        problems.Should().BeEmpty();
        roadSegment.IsRemoved.Should().BeTrue();
        ShouldHaveNoStreetNames(roadSegment);

        var changes = roadSegment.GetChanges().ToList();
        changes.Should().HaveCount(3);
        changes.Take(2).Should().AllBeOfType<RoadSegmentStreetNameIdWasChanged>();
        changes[2].Should().BeOfType<RoadSegmentWasRemoved>();
    }

    [Fact]
    public void WhenASegmentWithoutStreetNamesIsRemoved_ThenOnlyTheRemovalIsRecorded()
    {
        var roadNetwork = BuildRealizedNetwork();
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        roadSegment.UnlinkStreetNames(TestData.Provenance);
        roadSegment.WithoutChanges();

        roadSegment.Remove(TestData.Provenance);

        roadSegment.GetChanges().Should().ContainSingle().Which.Should().BeOfType<RoadSegmentWasRemoved>();
    }

    [Fact]
    public void WhenGivenStreetNamesOfARemovedSegmentAreLetGo_ThenOnlyThoseAreLetGo()
    {
        // The run-once correction: a removed road segment that kept its street names from before GAWR-7784.
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var eastNode = BuildNode(11, 200, 0, RoadNodeTypeV2.Eindknoop);
        var segmentAdded = WithStreetNames(BuildSegment(ChangedSegmentId, westNode, eastNode, BuildGeometry((0, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd));
        var removedSegment = RoadRegistry.RoadSegment.RoadSegment.Create(segmentAdded);
        removedSegment.Apply(new RoadSegmentWasRemoved { RoadSegmentId = segmentAdded.RoadSegmentId, Provenance = segmentAdded.Provenance });
        var roadNetwork = new ScopedRoadNetwork(
            new ScopedRoadNetworkId(Guid.NewGuid()),
            [],
            [removedSegment.WithoutChanges()],
            [],
            []).WithoutChanges();

        roadNetwork.UnlinkStreetNames(
            new Dictionary<RoadSegmentId, IReadOnlyCollection<StreetNameLocalId>> { [segmentAdded.RoadSegmentId] = [LeftStreetNameId] },
            TestData.Provenance);

        var @event = removedSegment.GetChanges().Should().ContainSingle().Which.Should().BeOfType<RoadSegmentStreetNameIdWasChanged>().Which;
        @event.OldStreetNameId.Should().Be(LeftStreetNameId);
        @event.NewStreetNameId.Should().Be(StreetNameLocalId.NotApplicable);
        removedSegment.Attributes!.StreetNameId.Values.Single(x => x.Side == RoadSegmentAttributeSide.Links).Value.Should().Be(StreetNameLocalId.NotApplicable);
        removedSegment.Attributes!.StreetNameId.Values.Single(x => x.Side == RoadSegmentAttributeSide.Rechts).Value.Should().Be(RightStreetNameId);
    }

    [Fact]
    public void WhenSeveralSegmentsAreRelinked_ThenEveryEventGetsAnOrdinalOfItsOwn()
    {
        // Saving a road network refuses two events with the same ordinal.
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(11, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var eastNode = BuildNode(12, 200, 0, RoadNodeTypeV2.Eindknoop);
        var roadNetwork = BuildNetwork(
            [westNode, middleNode, eastNode],
            [
                WithStreetNames(BuildSegment(ChangedSegmentId, westNode, middleNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd)),
                WithStreetNames(BuildSegment(NeighbourSegmentId, middleNode, eastNode, BuildGeometry((100, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd))
            ]);

        roadNetwork.ChangeStreetNameId(
            [new RoadSegmentId(ChangedSegmentId), new RoadSegmentId(NeighbourSegmentId)],
            LeftStreetNameId,
            StreetNameLocalId.NotApplicable,
            TestData.Provenance);

        var ordinals = roadNetwork.RoadSegments.Values.SelectMany(x => x.GetRecordedChanges()).Select(x => x.Ordinal).ToList();
        ordinals.Should().HaveCount(2);
        ordinals.Should().OnlyHaveUniqueItems();
    }
}
