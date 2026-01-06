namespace RoadRegistry.Projections.Tests.Projections.ExtractGradeSeparatedJunction;

using AutoFixture;
using FluentAssertions;
using GradeSeparatedJunction.Events.V2;
using JasperFx.Events;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.Tests.AggregateTests;

public class GradeSeparatedJunctionProjectionTests
{
    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        var excludeEventTypes = new[]
        {
            typeof(RoadRegistry.RoadNetwork.Events.V1.RoadNetworkChangesAccepted),
            typeof(RoadRegistry.RoadNode.Events.V1.ImportedRoadNode),
            typeof(RoadRegistry.RoadNode.Events.V1.RoadNodeAdded),
            typeof(RoadRegistry.RoadNode.Events.V1.RoadNodeModified),
            typeof(RoadRegistry.RoadNode.Events.V1.RoadNodeRemoved),
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

            typeof(RoadRegistry.RoadNetwork.Events.V2.RoadNetworkChanged),
            typeof(RoadRegistry.RoadNode.Events.V2.RoadNodeWasAdded),
            typeof(RoadRegistry.RoadNode.Events.V2.RoadNodeWasModified),
            typeof(RoadRegistry.RoadNode.Events.V2.RoadNodeWasMigrated),
            typeof(RoadRegistry.RoadNode.Events.V2.RoadNodeWasRemoved),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAdded),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAddedToEuropeanRoad),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasAddedToNationalRoad),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasModified),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasMerged),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasMigrated),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRetiredBecauseOfMerger),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRetiredBecauseOfMigration),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRemoved),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRemovedFromEuropeanRoad),
            typeof(RoadRegistry.RoadSegment.Events.V2.RoadSegmentWasRemovedFromNationalRoad)
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
    public Task WhenGradeSeparatedJunctionAdded_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestData().Fixture;

        var junction1Added = fixture.Create<GradeSeparatedJunctionWasAdded>();
        var junction2Added = fixture.Create<GradeSeparatedJunctionWasAdded>();

        var expectedJunction1 = new GradeSeparatedJunctionExtractItem
        {
            GradeSeparatedJunctionId = junction1Added.GradeSeparatedJunctionId,
            LowerRoadSegmentId = junction1Added.LowerRoadSegmentId,
            UpperRoadSegmentId = junction1Added.UpperRoadSegmentId,
            Type = junction1Added.Type,
            Origin = junction1Added.Provenance.ToEventTimestamp(),
            LastModified = junction1Added.Provenance.ToEventTimestamp()
        };
        var expectedJunction2 = new GradeSeparatedJunctionExtractItem
        {
            GradeSeparatedJunctionId = junction2Added.GradeSeparatedJunctionId,
            LowerRoadSegmentId = junction2Added.LowerRoadSegmentId,
            UpperRoadSegmentId = junction2Added.UpperRoadSegmentId,
            Type = junction2Added.Type,
            Origin = junction2Added.Provenance.ToEventTimestamp(),
            LastModified = junction2Added.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(junction1Added, junction2Added)
            .Expect(expectedJunction1, expectedJunction2);
    }

    [Fact]
    public Task WhenGradeSeparatedJunctionModified_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestData().Fixture;
        fixture.Freeze<GradeSeparatedJunctionId>();

        var junctionAdded = fixture.Create<GradeSeparatedJunctionWasAdded>();
        var junctionModified = fixture.Create<GradeSeparatedJunctionWasModified>();

        var expectedRoadNode = new GradeSeparatedJunctionExtractItem
        {
            GradeSeparatedJunctionId = junctionAdded.GradeSeparatedJunctionId,
            LowerRoadSegmentId = junctionModified.LowerRoadSegmentId!.Value,
            UpperRoadSegmentId = junctionModified.UpperRoadSegmentId!.Value,
            Type = junctionModified.Type!,
            Origin = junctionAdded.Provenance.ToEventTimestamp(),
            LastModified = junctionModified.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(junctionAdded, junctionModified)
            .Expect(expectedRoadNode);
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionRemoved_ThenNone()
    {
        var fixture = new RoadNetworkTestData().Fixture;
        fixture.Freeze<GradeSeparatedJunctionId>();

        var junction1Added = fixture.Create<GradeSeparatedJunctionWasAdded>();
        var junction1Removed = fixture.Create<GradeSeparatedJunctionWasRemoved>();

        await BuildProjection()
            .Scenario()
            .Given(junction1Added, junction1Removed)
            .ExpectNone();
    }

    private GradeSeparatedJunctionProjection BuildProjection()
    {
        return new GradeSeparatedJunctionProjection();
    }
}
