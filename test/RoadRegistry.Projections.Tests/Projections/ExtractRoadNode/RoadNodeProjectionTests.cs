namespace RoadRegistry.Projections.Tests.Projections.ExtractRoadNode;

using AutoFixture;
using Extracts.Projections;
using FluentAssertions;
using JasperFx.Events;
using RoadNode;
using RoadNode.Events.V2;
using RoadRegistry.Tests.AggregateTests;

public class RoadNodeProjectionTests
{
    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        var excludeEventTypes = new[]
        {
            typeof(RoadRegistry.RoadNetwork.Events.V1.RoadNetworkChangesAccepted),
            typeof(RoadRegistry.RoadSegment.Events.V1.ImportedRoadSegment),
            typeof(RoadRegistry.RoadSegment.Events.V1.OutlinedRoadSegmentRemoved),
            typeof(RoadRegistry.RoadSegment.Events.V1.RoadSegmentAdded),
            typeof(RoadRegistry.RoadSegment.Events.V1.RoadSegmentAddedToEuropeanRoad),
            typeof(RoadRegistry.RoadSegment.Events.V1.RoadSegmentAddedToNationalRoad),
            typeof(RoadRegistry.RoadSegment.Events.V1.RoadSegmentAddedToNumberedRoad),
            typeof(RoadRegistry.RoadSegment.Events.V1.RoadSegmentAttributesModified),
            typeof(RoadRegistry.RoadSegment.Events.V1.RoadSegmentGeometryModified),
            typeof(RoadRegistry.RoadSegment.Events.V1.RoadSegmentModified),
            typeof(RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemoved),
            typeof(RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemovedFromEuropeanRoad),
            typeof(RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemovedFromNationalRoad),
            typeof(RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemovedFromNumberedRoad),
            typeof(RoadRegistry.RoadSegment.Events.V1.RoadSegmentStreetNamesChanged),
            typeof(GradeSeparatedJunction.Events.V1.ImportedGradeSeparatedJunction),
            typeof(GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionAdded),
            typeof(GradeSeparatedJunction.Events.V1.GradeSeparatedJunctionRemoved),

            typeof(RoadRegistry.RoadNetwork.Events.V2.RoadNetworkChanged),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAddedToEuropeanRoad),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAddedToNationalRoad),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasModified),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasMerged),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRetiredBecauseOfMerger),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRemoved),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRemovedFromEuropeanRoad),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRemovedFromNationalRoad),
            typeof(GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasAdded),
            typeof(GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasModified),
            typeof(GradeSeparatedJunction.Events.V2.GradeSeparatedJunctionWasRemoved)
        };

        var allEventTypes = typeof(IMartenEvent).Assembly
            .GetTypes()
            .Where(x => typeof(IMartenEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .ToArray();
        allEventTypes.Should().NotBeEmpty();

        var usedEventTypes = BuildProjection().Handlers
            .Select(x => typeof(IEvent).IsAssignableFrom(x.Message) ? x.Message.GetGenericArguments().Single() : x.Message)
            .ToArray();
        usedEventTypes.Should().NotBeEmpty();
        usedEventTypes.Distinct().Count().Should().Be(usedEventTypes.Length);

        var missingEventTypes = allEventTypes.Except(usedEventTypes).Except(excludeEventTypes).ToArray();
        if (missingEventTypes.Any())
        {
            Assert.Fail($"Missing handlers for event types:{Environment.NewLine}{string.Join(Environment.NewLine, missingEventTypes.Select(x => x.FullName).OrderBy(x => x))}");
        }
    }

    [Fact]
    public Task WhenRoadNodeAdded_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestData().Fixture;

        var roadNode1Added = fixture.Create<RoadNodeWasAdded>();
        var roadNode2Added = fixture.Create<RoadNodeWasAdded>();

        var expectedRoadNode1 = new RoadNodeExtractItem
        {
            RoadNodeId = roadNode1Added.RoadNodeId,
            Geometry = roadNode1Added.Geometry,
            Type = roadNode1Added.Type,
            Origin = roadNode1Added.Provenance.ToEventTimestamp(),
            LastModified = roadNode1Added.Provenance.ToEventTimestamp()
        };
        var expectedRoadNode2 = new RoadNodeExtractItem
        {
            RoadNodeId = roadNode2Added.RoadNodeId,
            Geometry = roadNode2Added.Geometry,
            Type = roadNode2Added.Type,
            Origin = roadNode2Added.Provenance.ToEventTimestamp(),
            LastModified = roadNode2Added.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(roadNode1Added, roadNode2Added)
            .Expect(expectedRoadNode1, expectedRoadNode2);
    }

    [Fact]
    public Task WhenRoadNodeModified_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestData().Fixture;
        fixture.Freeze<RoadNodeId>();

        var roadNodeAdded = fixture.Create<RoadNodeWasAdded>();
        var roadNodeModified = fixture.Create<RoadNodeWasModified>();

        var expectedRoadNode = new RoadNodeExtractItem
        {
            RoadNodeId = roadNodeAdded.RoadNodeId,
            Geometry = roadNodeModified.Geometry,
            Type = roadNodeModified.Type!,
            Origin = roadNodeAdded.Provenance.ToEventTimestamp(),
            LastModified = roadNodeModified.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(roadNodeAdded, roadNodeModified)
            .Expect(expectedRoadNode);
    }

    [Fact]
    public async Task WhenRoadNodeRemoved_ThenNone()
    {
        var fixture = new RoadNetworkTestData().Fixture;
        fixture.Freeze<RoadNodeId>();

        var roadNode1Added = fixture.Create<RoadNodeWasAdded>();
        var roadNode1Removed = fixture.Create<RoadNodeWasRemoved>();

        await BuildProjection()
            .Scenario()
            .Given(roadNode1Added, roadNode1Removed)
            .ExpectNone();
    }

    private RoadNodeProjection BuildProjection()
    {
        return new RoadNodeProjection();
    }
}
