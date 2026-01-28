namespace RoadRegistry.Projections.Tests.Projections.ExtractRoadNode;

using AutoFixture;
using Extracts.Projections;
using FluentAssertions;
using GradeSeparatedJunction.Events.V1;
using GradeSeparatedJunction.Events.V2;
using JasperFx.Events;
using RoadNode.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadSegment.Events.V1;
using RoadSegment.Events.V2;
using ScopedRoadNetwork.Events.V1;
using ScopedRoadNetwork.Events.V2;

public class RoadNodeProjectionTests
{
    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        var excludeEventTypes = new[]
        {
            typeof(RoadNetworkChangesAccepted),
            typeof(ImportedRoadSegment),
            typeof(OutlinedRoadSegmentRemoved),
            typeof(RoadSegmentAdded),
            typeof(RoadSegmentAddedToEuropeanRoad),
            typeof(RoadSegmentAddedToNationalRoad),
            typeof(RoadSegmentAddedToNumberedRoad),
            typeof(RoadSegmentAttributesModified),
            typeof(RoadSegmentGeometryModified),
            typeof(RoadSegmentModified),
            typeof(RoadSegmentRemoved),
            typeof(RoadSegmentRemovedFromEuropeanRoad),
            typeof(RoadSegmentRemovedFromNationalRoad),
            typeof(RoadSegmentRemovedFromNumberedRoad),
            typeof(RoadSegmentStreetNamesChanged),
            typeof(ImportedGradeSeparatedJunction),
            typeof(GradeSeparatedJunctionAdded),
            typeof(GradeSeparatedJunctionModified),
            typeof(GradeSeparatedJunctionRemoved),

            typeof(RoadNetworkWasChanged),
            typeof(RoadSegmentWasAdded),
            typeof(RoadSegmentWasAddedToEuropeanRoad),
            typeof(RoadSegmentWasAddedToNationalRoad),
            typeof(RoadSegmentWasModified),
            typeof(RoadSegmentWasMerged),
            typeof(RoadSegmentWasMigrated),
            typeof(RoadSegmentWasRetiredBecauseOfMerger),
            typeof(RoadSegmentWasRetiredBecauseOfMigration),
            typeof(RoadSegmentWasRemoved),
            typeof(RoadSegmentWasRemovedFromEuropeanRoad),
            typeof(RoadSegmentWasRemovedFromNationalRoad),
            typeof(GradeSeparatedJunctionWasAdded),
            typeof(GradeSeparatedJunctionWasModified),
            typeof(GradeSeparatedJunctionWasRemoved)
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
    public Task WhenRoadNodeWasAdded_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var roadNode1Added = fixture.Create<RoadNodeWasAdded>();
        var roadNode2Added = fixture.Create<RoadNodeWasAdded>();

        var expectedRoadNode1 = new RoadNodeExtractItem
        {
            RoadNodeId = roadNode1Added.RoadNodeId,
            Geometry = roadNode1Added.Geometry,
            Type = roadNode1Added.Type,
            Grensknoop = roadNode1Added.Grensknoop,
            IsV2 = true,
            Origin = roadNode1Added.Provenance.ToEventTimestamp(),
            LastModified = roadNode1Added.Provenance.ToEventTimestamp()
        };
        var expectedRoadNode2 = new RoadNodeExtractItem
        {
            RoadNodeId = roadNode2Added.RoadNodeId,
            Geometry = roadNode2Added.Geometry,
            Type = roadNode2Added.Type,
            Grensknoop = roadNode2Added.Grensknoop,
            IsV2 = true,
            Origin = roadNode2Added.Provenance.ToEventTimestamp(),
            LastModified = roadNode2Added.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(roadNode1Added, roadNode2Added)
            .Expect(expectedRoadNode1, expectedRoadNode2);
    }

    [Fact]
    public Task WhenRoadNodeWasModified_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadNodeId>();

        var roadNodeAdded = fixture.Create<RoadNodeWasAdded>();
        var roadNodeModified = fixture.Create<RoadNodeWasModified>();

        var expectedRoadNode = new RoadNodeExtractItem
        {
            RoadNodeId = roadNodeAdded.RoadNodeId,
            Geometry = roadNodeModified.Geometry,
            Type = roadNodeModified.Type!,
            Grensknoop = roadNodeModified.Grensknoop!.Value,
            IsV2 = true,
            Origin = roadNodeAdded.Provenance.ToEventTimestamp(),
            LastModified = roadNodeModified.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(roadNodeAdded, roadNodeModified)
            .Expect(expectedRoadNode);
    }

    [Fact]
    public Task WhenRoadNodeWasMigrated_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<RoadNodeId>();

        var roadNodeAdded = fixture.Create<RoadNodeWasAdded>();
        var roadNodeMigrated = fixture.Create<RoadNodeWasMigrated>();

        var expectedRoadNode = new RoadNodeExtractItem
        {
            RoadNodeId = roadNodeAdded.RoadNodeId,
            Geometry = roadNodeMigrated.Geometry,
            Type = roadNodeMigrated.Type,
            Grensknoop = roadNodeMigrated.Grensknoop,
            IsV2 = true,
            Origin = roadNodeAdded.Provenance.ToEventTimestamp(),
            LastModified = roadNodeMigrated.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(roadNodeAdded, roadNodeMigrated)
            .Expect(expectedRoadNode);
    }

    [Fact]
    public async Task WhenRoadNodeWasRemoved_ThenNone()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
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
