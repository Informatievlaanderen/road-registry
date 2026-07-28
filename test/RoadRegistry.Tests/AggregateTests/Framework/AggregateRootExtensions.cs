namespace RoadRegistry.Tests.AggregateTests.Framework;

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ScopedRoadNetwork;

public static class AggregateRootExtensions
{
    // The raw uncommitted events of an aggregate, for assertions. Production code only needs the ordinal-carrying
    // GetRecordedChanges(); this test-only view drops the ordinal.
    public static IReadOnlyCollection<object> GetChanges(this IMartenAggregateRootEntity aggregate)
    {
        return aggregate.GetRecordedChanges().Select(x => (object)x.Event).ToArray();
    }

    public static ScopedRoadNetwork WithoutChanges(this ScopedRoadNetwork aggregate)
    {
        aggregate.ClearUncommittedEvents();
        foreach (var childAggregate in aggregate.RoadNodes.Values)
        {
            childAggregate.ClearUncommittedEvents();
        }
        foreach (var childAggregate in aggregate.RoadSegments.Values)
        {
            childAggregate.ClearUncommittedEvents();
        }
        foreach (var childAggregate in aggregate.GradeSeparatedJunctions.Values)
        {
            childAggregate.ClearUncommittedEvents();
        }
        foreach (var childAggregate in aggregate.GradeJunctions.Values)
        {
            childAggregate.ClearUncommittedEvents();
        }
        return aggregate;
    }
    public static RoadRegistry.RoadNode.RoadNode WithoutChanges(this RoadRegistry.RoadNode.RoadNode aggregate)
    {
        aggregate.ClearUncommittedEvents();
        return aggregate;
    }
    public static RoadRegistry.RoadSegment.RoadSegment WithoutChanges(this RoadRegistry.RoadSegment.RoadSegment aggregate)
    {
        aggregate.ClearUncommittedEvents();
        return aggregate;
    }
    public static RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction WithoutChanges(this RoadRegistry.GradeSeparatedJunction.GradeSeparatedJunction aggregate)
    {
        aggregate.ClearUncommittedEvents();
        return aggregate;
    }
    public static RoadRegistry.GradeJunction.GradeJunction WithoutChanges(this RoadRegistry.GradeJunction.GradeJunction aggregate)
    {
        aggregate.ClearUncommittedEvents();
        return aggregate;
    }
    private static void ClearUncommittedEvents<TIdentifier>(this MartenAggregateRootEntity<TIdentifier> aggregate)
    {
        var uncommittedEvents = (UncommittedEventCollection)aggregate.GetType()
            .GetProperty("UncommittedEvents", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(aggregate)!;
        uncommittedEvents.Clear();
    }
}
