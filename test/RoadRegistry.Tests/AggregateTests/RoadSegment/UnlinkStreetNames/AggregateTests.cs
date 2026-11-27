namespace RoadRegistry.Tests.AggregateTests.RoadSegment.UnlinkStreetNames;

using System.Linq;
using AutoFixture;
using FluentAssertions;
using NetTopologySuite.Geometries;
using RoadRegistry.Extensions;
using RoadRegistry.RoadNetwork.Schema;
using RoadRegistry.RoadSegment.Changes;
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
        changes.Skip(1).Cast<RoadSegmentStreetNameWasUnlinked>()
            .Select(x => x.StreetNameId)
            .Should().BeEquivalentTo([LeftStreetNameId, RightStreetNameId]);
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
        roadSegment.GetChanges().OfType<RoadSegmentStreetNameWasUnlinked>().Should().BeEmpty();
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
        roadSegment.GetChanges().OfType<RoadSegmentStreetNameWasUnlinked>().Should().HaveCount(2);
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
        roadSegment.GetChanges().Last().Should().BeOfType<RoadSegmentStreetNameWasUnlinked>();
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
    public void WhenASegmentWithStreetNamesIsRemoved_ThenOnlyTheRemovalIsRecorded()
    {
        // Removing is letting go of the street names by definition: no RoadSegmentStreetNameWasUnlinked precedes it, and
        // the aggregate keeps the attribute as it was.
        var roadNetwork = BuildRealizedNetwork();
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        var streetNamesBefore = roadSegment.Attributes!.StreetNameId;

        var problems = roadSegment.Remove(TestData.Provenance);

        problems.Should().BeEmpty();
        roadSegment.IsRemoved.Should().BeTrue();
        roadSegment.Attributes!.StreetNameId.Should().Be(streetNamesBefore);
        roadSegment.GetChanges().Should().ContainSingle().Which.Should().BeOfType<RoadSegmentWasRemoved>();
    }

    [Fact]
    public void WhenSeveralSegmentsAreRelinked_ThenEveryEventGetsAnOrdinalOfItsOwn()
    {
        // Saving a road network refuses two events with the same ordinal.
        var roadNetwork = BuildNetworkWithTwoLinkedSegments();

        roadNetwork.RelinkStreetName(BothSegmentIds, LeftStreetNameId, OtherStreetNameId, TestData.Provenance);

        var ordinals = roadNetwork.RoadSegments.Values.SelectMany(x => x.GetRecordedChanges()).Select(x => x.Ordinal).ToList();
        ordinals.Should().HaveCount(2);
        ordinals.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void WhenSeveralSegmentsAreUnlinked_ThenEveryEventGetsAnOrdinalOfItsOwn()
    {
        var roadNetwork = BuildNetworkWithTwoLinkedSegments();

        roadNetwork.UnlinkStreetName(BothSegmentIds, LeftStreetNameId, TestData.Provenance);

        var ordinals = roadNetwork.RoadSegments.Values.SelectMany(x => x.GetRecordedChanges()).Select(x => x.Ordinal).ToList();
        ordinals.Should().HaveCount(2);
        ordinals.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void WhenAStreetNameIsRelinked_ThenTheSegmentFollowsItToTheNewStreetName()
    {
        var roadNetwork = BuildNetworkWithTwoLinkedSegments();
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];

        roadNetwork.RelinkStreetName([roadSegment.RoadSegmentId], LeftStreetNameId, OtherStreetNameId, TestData.Provenance);

        var @event = roadSegment.GetChanges().Should().ContainSingle().Which.Should().BeOfType<RoadSegmentStreetNameWasRelinked>().Which;
        @event.OldStreetNameId.Should().Be(LeftStreetNameId);
        @event.NewStreetNameId.Should().Be(OtherStreetNameId);
        @event.StreetNameIdAttributeValues.Should().Be(roadSegment.Attributes!.StreetNameId);
        roadSegment.Attributes.StreetNameId.Values.Single(x => x.Side == RoadSegmentAttributeSide.Links).Value.Should().Be(OtherStreetNameId);
        roadSegment.Attributes.StreetNameId.Values.Single(x => x.Side == RoadSegmentAttributeSide.Rechts).Value.Should().Be(RightStreetNameId);
    }

    [Fact]
    public void WhenAStreetNameIsRelinkedToNotApplicable_ThenThatIsADefect()
    {
        // Letting go of a street name is an unlink, never a relink to 'niet van toepassing'.
        var roadNetwork = BuildNetworkWithTwoLinkedSegments();
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];

        var act = () => roadSegment.RelinkStreetName(LeftStreetNameId, StreetNameLocalId.NotApplicable, TestData.Provenance);

        act.Should().Throw<System.InvalidOperationException>();
        roadSegment.GetChanges().Should().BeEmpty();
    }

    [Fact]
    public void WhenAStreetNameIsUnlinked_ThenOnlyThatStreetNameIsLetGo()
    {
        var roadNetwork = BuildNetworkWithTwoLinkedSegments();
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];

        roadNetwork.UnlinkStreetName([roadSegment.RoadSegmentId], LeftStreetNameId, TestData.Provenance);

        var @event = roadSegment.GetChanges().Should().ContainSingle().Which.Should().BeOfType<RoadSegmentStreetNameWasUnlinked>().Which;
        @event.StreetNameId.Should().Be(LeftStreetNameId);
        @event.StreetNameIdAttributeValues.Should().Be(roadSegment.Attributes!.StreetNameId);
        roadSegment.Attributes.StreetNameId.Values.Single(x => x.Side == RoadSegmentAttributeSide.Links).Value.Should().Be(StreetNameLocalId.NotApplicable);
        roadSegment.Attributes.StreetNameId.Values.Single(x => x.Side == RoadSegmentAttributeSide.Rechts).Value.Should().Be(RightStreetNameId);
    }

    [Fact]
    public void WhenAStreetNameTheSegmentDoesNotCarryIsUnlinked_ThenNothingIsRecorded()
    {
        var roadNetwork = BuildNetworkWithTwoLinkedSegments();
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];

        roadNetwork.UnlinkStreetName([roadSegment.RoadSegmentId], OtherStreetNameId, TestData.Provenance);

        roadSegment.GetChanges().Should().BeEmpty();
    }

    // GAWR-7784, follow-up: a road segment that is added or migrated in a status that is not active, is recorded without
    // street names - 'niet gekend' (-8) included - rather than with them and then let go of them.
    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.Gehistoreerd))]
    [InlineData(nameof(RoadSegmentStatusV2.NietGerealiseerd))]
    public void WhenAnInactiveSegmentIsAdded_ThenItIsAddedWithoutStreetNames(string status)
    {
        var geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates().ToRoadSegmentGeometry();
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Parse(status),
            Geometry = geometry,
            StreetNameId = StreetNamesWithUnknown(geometry),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };
        var roadNetwork = new RoadNetworkBuilder(new InMemoryRoadNetworkIdGenerator()).Build();

        var (roadSegment, problems) = RoadRegistry.RoadSegment.RoadSegment.Add(change, new InMemoryRoadNetworkIdGenerator(), new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance));

        problems.Should().HaveNoError();
        roadSegment!.GetChanges().Should().ContainSingle().Which.Should().BeOfType<RoadSegmentWasAdded>()
            .Which.StreetNameId.Values.Should().OnlyContain(x => x.Value == StreetNameLocalId.NotApplicable);
    }

    [Fact]
    public void WhenAnActiveSegmentIsAdded_ThenItKeepsItsStreetNames()
    {
        var geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates().ToRoadSegmentGeometry();
        var change = Fixture.Create<AddRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Gepland,
            Geometry = geometry,
            StreetNameId = StreetNamesWithUnknown(geometry),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };
        var roadNetwork = new RoadNetworkBuilder(new InMemoryRoadNetworkIdGenerator()).Build();

        var (roadSegment, problems) = RoadRegistry.RoadSegment.RoadSegment.Add(change, new InMemoryRoadNetworkIdGenerator(), new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance));

        problems.Should().HaveNoError();
        roadSegment!.GetChanges().Should().ContainSingle().Which.Should().BeOfType<RoadSegmentWasAdded>()
            .Which.StreetNameId.Should().Be(change.StreetNameId);
    }

    [Theory]
    [InlineData(nameof(RoadSegmentStatusV2.Gehistoreerd))]
    [InlineData(nameof(RoadSegmentStatusV2.NietGerealiseerd))]
    public void WhenAnInactiveSegmentIsMigrated_ThenItIsMigratedWithoutStreetNames(string status)
    {
        Fixture.Freeze<RoadSegmentId>();
        var roadSegment = RoadRegistry.RoadSegment.RoadSegment.Create(Fixture.Create<RoadSegmentWasAdded>()).WithoutChanges();
        var geometry = Fixture.Create<MultiLineString>().WithMeasureOrdinates().ToRoadSegmentGeometry();
        var change = Fixture.Create<MigrateRoadSegmentChange>() with
        {
            Status = RoadSegmentStatusV2.Parse(status),
            Geometry = geometry,
            StreetNameId = StreetNamesWithUnknown(geometry),
            EuropeanRoadNumbers = [],
            NationalRoadNumbers = []
        };
        var roadNetwork = new RoadNetworkBuilder(new InMemoryRoadNetworkIdGenerator()).Build();

        var problems = roadSegment.Migrate(change, new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance));

        problems.Should().HaveNoError();
        roadSegment.GetChanges().Should().ContainSingle().Which.Should().BeOfType<RoadSegmentWasMigrated>()
            .Which.StreetNameId.Values.Should().OnlyContain(x => x.Value == StreetNameLocalId.NotApplicable);
    }

    [Fact]
    public void WhenAnUploadMakesASegmentWithAnUnknownStreetNameInactive_ThenTheUnknownStreetNameIsLetGoAsWell()
    {
        var roadNetwork = BuildUnconnectedNetworkWith(RoadSegmentStatusV2.Gepland);
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        var change = new RoadRegistry.RoadSegment.Changes.ModifyRoadSegmentChange
        {
            RoadSegmentIdReference = new RoadSegmentIdReference(roadSegment.RoadSegmentId),
            Status = RoadSegmentStatusV2.NietGerealiseerd,
            StreetNameId = StreetNamesWithUnknown(roadSegment.Geometry)
        };

        var problems = roadSegment.Modify(change, new ScopedRoadNetworkChangeContext(roadNetwork, TestData.Provenance));

        problems.Should().HaveNoError();
        ShouldHaveNoStreetNames(roadSegment);

        // The street names are recorded on RoadSegmentWasChanged as they came, and let go of after it.
        var changes = roadSegment.GetChanges().ToList();
        changes.OfType<RoadSegmentWasChanged>().Should().ContainSingle()
            .Which.StreetNameId.Should().Be(change.StreetNameId);
        changes.SkipWhile(x => x is not RoadSegmentWasChanged).Skip(1).Cast<RoadSegmentStreetNameWasUnlinked>()
            .Select(x => x.StreetNameId)
            .Should().BeEquivalentTo([LeftStreetNameId, StreetNameLocalId.Unknown]);
    }

    [Fact]
    public void WhenAnActiveSegmentLetsGoOfAllItsStreetNamesTwice_ThenNothingIsRecordedTheSecondTime()
    {
        var roadNetwork = BuildRealizedNetwork();
        var roadSegment = roadNetwork.RoadSegments[new RoadSegmentId(ChangedSegmentId)];
        roadSegment.UnlinkStreetNames(TestData.Provenance);
        roadSegment.WithoutChanges();

        roadSegment.UnlinkStreetNames(TestData.Provenance);

        roadSegment.GetChanges().Should().BeEmpty();
    }

    // Street name 501 on the left, 'niet gekend' (-8) on the right.
    private static RoadSegmentDynamicAttributeValues<StreetNameLocalId> StreetNamesWithUnknown(RoadSegmentGeometry geometry)
    {
        var to = new RoadSegmentPositionV2(geometry.Value.Length);
        return new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
            .Add(RoadSegmentPositionV2.Zero, to, RoadSegmentAttributeSide.Links, LeftStreetNameId)
            .Add(RoadSegmentPositionV2.Zero, to, RoadSegmentAttributeSide.Rechts, StreetNameLocalId.Unknown);
    }

    private static readonly StreetNameLocalId OtherStreetNameId = new(503);

    private static readonly RoadSegmentId[] BothSegmentIds = [new(ChangedSegmentId), new(NeighbourSegmentId)];

    private ScopedRoadNetwork BuildNetworkWithTwoLinkedSegments()
    {
        var westNode = BuildNode(10, 0, 0, RoadNodeTypeV2.Eindknoop);
        var middleNode = BuildNode(11, 100, 0, RoadNodeTypeV2.Validatieknoop);
        var eastNode = BuildNode(12, 200, 0, RoadNodeTypeV2.Eindknoop);
        return BuildNetwork(
            [westNode, middleNode, eastNode],
            [
                WithStreetNames(BuildSegment(ChangedSegmentId, westNode, middleNode, BuildGeometry((0, 0), (100, 0)), RoadSegmentStatusV2.Gerealiseerd)),
                WithStreetNames(BuildSegment(NeighbourSegmentId, middleNode, eastNode, BuildGeometry((100, 0), (200, 0)), RoadSegmentStatusV2.Gerealiseerd))
            ]);
    }
}
