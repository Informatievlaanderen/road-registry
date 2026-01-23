namespace RoadRegistry.Projections.Tests.Projections.ExtractGradeSeparatedJunction;

using AutoFixture;
using Extracts.Projections;
using FluentAssertions;
using GradeSeparatedJunction.Events.V2;
using JasperFx.Events;
using RoadNode.Events.V1;
using RoadNode.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using RoadSegment.Events.V1;
using RoadSegment.Events.V2;
using ScopedRoadNetwork.Events.V1;
using ScopedRoadNetwork.Events.V2;

public class GradeSeparatedJunctionProjectionTests
{
    [Fact]
    public void EnsureAllEventsAreHandledExactlyOnce()
    {
        var excludeEventTypes = new[]
        {
            typeof(RoadNetworkChangesAccepted),
            typeof(ImportedRoadNode),
            typeof(RoadNodeAdded),
            typeof(RoadNodeModified),
            typeof(RoadNodeRemoved),
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

            typeof(RoadNetworkWasChanged),
            typeof(RoadNodeWasAdded),
            typeof(RoadNodeWasModified),
            typeof(RoadNodeWasMigrated),
            typeof(RoadNodeWasRemoved),
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
            typeof(RoadSegmentWasRemovedFromNationalRoad)
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
    public Task WhenGradeSeparatedJunctionWasAdded_ThenSucceeded()
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
            IsV2 = true,
            Origin = junction1Added.Provenance.ToEventTimestamp(),
            LastModified = junction1Added.Provenance.ToEventTimestamp()
        };
        var expectedJunction2 = new GradeSeparatedJunctionExtractItem
        {
            GradeSeparatedJunctionId = junction2Added.GradeSeparatedJunctionId,
            LowerRoadSegmentId = junction2Added.LowerRoadSegmentId,
            UpperRoadSegmentId = junction2Added.UpperRoadSegmentId,
            Type = junction2Added.Type,
            IsV2 = true,
            Origin = junction2Added.Provenance.ToEventTimestamp(),
            LastModified = junction2Added.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(junction1Added, junction2Added)
            .Expect(expectedJunction1, expectedJunction2);
    }

    [Fact]
    public Task WhenGradeSeparatedJunctionWasModified_ThenSucceeded()
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
            IsV2 = true,
            Origin = junctionAdded.Provenance.ToEventTimestamp(),
            LastModified = junctionModified.Provenance.ToEventTimestamp()
        };

        return BuildProjection()
            .Scenario()
            .Given(junctionAdded, junctionModified)
            .Expect(expectedRoadNode);
    }

    [Fact]
    public async Task WhenGradeSeparatedJunctionWasRemoved_ThenNone()
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
