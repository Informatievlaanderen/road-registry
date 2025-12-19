namespace RoadRegistry.Tests.AggregateTests.Framework;

using System.Collections;
using System.Reflection;

public static class AggregateRootExtensions
{
    public static RoadRegistry.RoadNetwork.RoadNetwork WithoutChanges(this RoadRegistry.RoadNetwork.RoadNetwork aggregate)
    {
        aggregate.ClearUncommittedEvents();
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
    private static void ClearUncommittedEvents<TIdentifier>(this MartenAggregateRootEntity<TIdentifier> aggregate)
    {
        ((IList)aggregate.GetType().GetProperty("UncommittedEvents", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(aggregate))!.Clear();
    }
}
