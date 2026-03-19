namespace RoadRegistry.Projections.Tests.Projections.ExtractGradeJunction;

using AutoFixture;
using FluentAssertions;
using JasperFx.Events;
using RoadRegistry.Extracts.Projections;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.Tests.AggregateTests;
using ImportedRoadNode = RoadRegistry.RoadNode.Events.V1.ImportedRoadNode;
using ImportedRoadSegment = RoadRegistry.RoadSegment.Events.V1.ImportedRoadSegment;
using OutlinedRoadSegmentRemoved = RoadRegistry.RoadSegment.Events.V1.OutlinedRoadSegmentRemoved;
using RoadNetworkChangesAccepted = RoadRegistry.ScopedRoadNetwork.Events.V1.RoadNetworkChangesAccepted;
using RoadNodeAdded = RoadRegistry.RoadNode.Events.V1.RoadNodeAdded;
using RoadNodeModified = RoadRegistry.RoadNode.Events.V1.RoadNodeModified;
using RoadNodeRemoved = RoadRegistry.RoadNode.Events.V1.RoadNodeRemoved;
using RoadSegmentAdded = RoadRegistry.RoadSegment.Events.V1.RoadSegmentAdded;
using RoadSegmentAddedToEuropeanRoad = RoadRegistry.RoadSegment.Events.V1.RoadSegmentAddedToEuropeanRoad;
using RoadSegmentAddedToNationalRoad = RoadRegistry.RoadSegment.Events.V1.RoadSegmentAddedToNationalRoad;
using RoadSegmentAddedToNumberedRoad = RoadRegistry.RoadSegment.Events.V1.RoadSegmentAddedToNumberedRoad;
using RoadSegmentAttributesModified = RoadRegistry.RoadSegment.Events.V1.RoadSegmentAttributesModified;
using RoadSegmentGeometryModified = RoadRegistry.RoadSegment.Events.V1.RoadSegmentGeometryModified;
using RoadSegmentModified = RoadRegistry.RoadSegment.Events.V1.RoadSegmentModified;
using RoadSegmentRemoved = RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemoved;
using RoadSegmentRemovedFromEuropeanRoad = RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemovedFromEuropeanRoad;
using RoadSegmentRemovedFromNationalRoad = RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemovedFromNationalRoad;
using RoadSegmentRemovedFromNumberedRoad = RoadRegistry.RoadSegment.Events.V1.RoadSegmentRemovedFromNumberedRoad;
using RoadSegmentStreetNamesChanged = RoadRegistry.RoadSegment.Events.V1.RoadSegmentStreetNamesChanged;

public class GradeJunctionProjectionTests
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
            typeof(ImportedGradeSeparatedJunction),
            typeof(GradeSeparatedJunctionAdded),
            typeof(GradeSeparatedJunctionModified),
            typeof(GradeSeparatedJunctionRemoved),

            typeof(RoadNetworkWasChanged),
            typeof(RoadNodeWasAdded),
            typeof(RoadNodeTypeWasChanged),
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
    public Task WhenGradeJunctionWasAdded_ThenSucceeded()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;

        var junction1Added = fixture.Create<GradeJunctionWasAdded>();
        var junction2Added = fixture.Create<GradeJunctionWasAdded>();

        var expectedJunction1 = new GradeJunctionExtractItem
        {
            GradeJunctionId = junction1Added.GradeJunctionId,
            RoadSegmentId1 = junction1Added.RoadSegmentId1,
            RoadSegmentId2 = junction1Added.RoadSegmentId2,
            IsV2 = true,
            Origin = junction1Added.Provenance.ToEventTimestamp(),
            LastModified = junction1Added.Provenance.ToEventTimestamp()
        };
        var expectedJunction2 = new GradeJunctionExtractItem
        {
            GradeJunctionId = junction2Added.GradeJunctionId,
            RoadSegmentId1 = junction2Added.RoadSegmentId1,
            RoadSegmentId2 = junction2Added.RoadSegmentId2,
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
    public async Task WhenGradeJunctionWasRemoved_ThenNone()
    {
        var fixture = new RoadNetworkTestDataV2().Fixture;
        fixture.Freeze<GradeJunctionId>();

        var junction1Added = fixture.Create<GradeJunctionWasAdded>();
        var junction1Removed = fixture.Create<GradeJunctionWasRemoved>();

        await BuildProjection()
            .Scenario()
            .Given(junction1Added, junction1Removed)
            .ExpectNone();
    }

    private GradeJunctionProjection BuildProjection()
    {
        return new GradeJunctionProjection();
    }
}
