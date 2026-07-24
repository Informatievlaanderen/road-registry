namespace RoadRegistry.Projections.Tests.Projections.WmsWfsV2;

using System;
using System.Linq;
using FluentAssertions;
using JasperFx.Events;
using RoadRegistry.GradeJunction.Events.V2;
using RoadRegistry.GradeSeparatedJunction.Events.V1;
using RoadRegistry.GradeSeparatedJunction.Events.V2;
using RoadRegistry.Infrastructure.MartenDb.Projections;
using RoadRegistry.Organization.Events.V2;
using RoadRegistry.WmsWfsV2.Projections;
using RoadRegistry.WmsWfsV2.Schema;
using RoadRegistry.RoadNode.Events.V1;
using RoadRegistry.RoadNode.Events.V2;
using RoadRegistry.RoadSegment.Events.V1;
using RoadRegistry.RoadSegment.Events.V2;
using RoadRegistry.ScopedRoadNetwork.Events.V1;
using RoadRegistry.ScopedRoadNetwork.Events.V2;
using RoadRegistry.StreetName.Events.V2;

// Shared support for the per-projection EnsureAllEventsAreHandledExactlyOnce tests (see each inner projection's test
// class). Every inner PBS projection declares exactly the events it is responsible for; this helper asserts that its
// registered handlers match that declaration one-for-one and that no road-network domain event has slipped through
// uncategorised. Adding a new IMartenEvent to the domain fails every projection's coverage test until the event is
// added to AllRoadNetworkEvents below AND either handled by the projection or consciously left out of its declaration.
internal static class WmsWfsV2ProjectionEventCoverage
{
    // The complete set of road-network domain events (all concrete IMartenEvent types). Kept in sync with the domain by
    // the reflection guard in AssertHandledExactlyOnce.
    private static readonly Type[] AllRoadNetworkEvents =
    [
        // RoadNode V1
        typeof(ImportedRoadNode), typeof(RoadNodeAdded), typeof(RoadNodeModified), typeof(RoadNodeRemoved),
        // RoadNode V2
        typeof(RoadNodeWasAdded), typeof(RoadNodeTypeWasChanged), typeof(RoadNodeWasModified), typeof(RoadNodeWasMigrated),
        typeof(RoadNodeWasRemoved), typeof(RoadNodeWasRemovedBecauseOfMigration),
        // RoadSegment V1
        typeof(ImportedRoadSegment), typeof(OutlinedRoadSegmentRemoved), typeof(RoadSegmentAdded),
        typeof(RoadSegmentAddedToEuropeanRoad), typeof(RoadSegmentAddedToNationalRoad), typeof(RoadSegmentAddedToNumberedRoad),
        typeof(RoadSegmentAttributesModified), typeof(RoadSegmentGeometryModified), typeof(RoadSegmentModified),
        typeof(RoadSegmentRemoved), typeof(RoadSegmentRemovedFromEuropeanRoad), typeof(RoadSegmentRemovedFromNationalRoad),
        typeof(RoadSegmentRemovedFromNumberedRoad), typeof(RoadSegmentStreetNamesChanged),
        // RoadSegment V2
        typeof(OutlinedRoadSegmentWasAdded), typeof(RoadSegmentGeometryWasModified), typeof(RoadSegmentStreetNameIdWasChanged),
        typeof(RoadSegmentWasAdded), typeof(RoadSegmentWasAddedToEuropeanRoad), typeof(RoadSegmentWasAddedToNationalRoad),
        typeof(RoadSegmentWasMerged), typeof(RoadSegmentWasMigrated), typeof(RoadSegmentWasModified),
        typeof(RoadSegmentWasRemoved), typeof(RoadSegmentWasRemovedBecauseOfMigration), typeof(RoadSegmentWasRemovedFromEuropeanRoad),
        typeof(RoadSegmentWasRemovedFromNationalRoad), typeof(RoadSegmentWasRetired), typeof(RoadSegmentWasRetiredBecauseOfMerger),
        typeof(RoadSegmentWasRetiredBecauseOfSplit), typeof(RoadSegmentWasSplit),
        // GradeJunction V2
        typeof(GradeJunctionWasAdded), typeof(GradeJunctionWasModified), typeof(GradeJunctionGeometryWasChanged), typeof(GradeJunctionWasRemoved),
        // GradeSeparatedJunction V1
        typeof(ImportedGradeSeparatedJunction), typeof(GradeSeparatedJunctionAdded), typeof(GradeSeparatedJunctionModified),
        typeof(GradeSeparatedJunctionRemoved), typeof(GradeSeparatedJunctionGeometryModified),
        // GradeSeparatedJunction V2
        typeof(GradeSeparatedJunctionWasAdded), typeof(GradeSeparatedJunctionWasModified), typeof(GradeSeparatedJunctionGeometryWasChanged),
        typeof(GradeSeparatedJunctionWasRemoved), typeof(GradeSeparatedJunctionWasRemovedBecauseOfMigration),
        // Organization V2
        typeof(OrganizationWasImported), typeof(OrganizationWasCreated), typeof(OrganizationWasModified), typeof(OrganizationWasRemoved),
        // StreetName V2
        typeof(StreetNameWasCreated), typeof(StreetNameWasModified), typeof(StreetNameWasRemoved), typeof(StreetNameWasRenamed),
        // ScopedRoadNetwork (not projected by PBS)
        typeof(RoadNetworkChangesAccepted), typeof(MunicipalityWasMigrated), typeof(RoadNetworkWasChangedBecauseOfExtract)
    ];

    // Each projection passes the events it deliberately does NOT handle (the exclude list); every remaining known event
    // must be handled. Adding a new domain event therefore fails every projection's test until it is either handled or
    // consciously added to that projection's exclude list.
    public static void AssertHandledExactlyOnce(RunnerDbContextRoadNetworkChangesProjection<WmsWfsV2Context> projection, params Type[] excludeEventTypes)
    {
        // First make sure our AllRoadNetworkEvents list still mirrors the domain, so a newly added event cannot pass
        // unnoticed through the per-projection checks below.
        var domainEvents = typeof(IMartenEvent).Assembly
            .GetTypes()
            .Where(x => typeof(IMartenEvent).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract)
            .ToArray();
        var unlisted = domainEvents.Except(AllRoadNetworkEvents).ToArray();
        if (unlisted.Any())
        {
            Assert.Fail($"New domain event(s) not accounted for in {nameof(WmsWfsV2ProjectionEventCoverage)}.{nameof(AllRoadNetworkEvents)}:{Environment.NewLine}{string.Join(Environment.NewLine, unlisted.Select(x => x.FullName).OrderBy(x => x))}");
        }

        var usedEventTypes = projection.Handlers
            .Select(x => typeof(IEvent).IsAssignableFrom(x.Message) ? x.Message.GetGenericArguments().Single() : x.Message)
            .ToArray();

        usedEventTypes.Should().NotBeEmpty();

        // Each event is handled at most once by this projection.
        usedEventTypes.Distinct().Count().Should().Be(usedEventTypes.Length, "each event must be handled exactly once");

        // Every known event that is neither handled nor explicitly excluded is a coverage gap.
        var missingEventTypes = AllRoadNetworkEvents.Except(usedEventTypes).Except(excludeEventTypes).ToArray();
        if (missingEventTypes.Any())
        {
            Assert.Fail($"Missing handlers for event types:{Environment.NewLine}{string.Join(Environment.NewLine, missingEventTypes.Select(x => x.FullName).OrderBy(x => x))}");
        }
    }
}
