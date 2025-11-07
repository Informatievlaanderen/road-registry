namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using System.Collections.Generic;
using System.Linq;

public static class ChangeExtensions
{
    public static IEnumerable<object> Flatten(this IEnumerable<ChangeRoadNetworkCommandItem> changes)
    {
        return changes.Select(Flatten);
    }

    private static object Flatten(this ChangeRoadNetworkCommandItem change)
    {
        return new object[]
            {
                change.AddRoadNode,
                change.ModifyRoadNode,
                change.RemoveRoadNode,
                change.AddRoadSegment,
                change.ModifyRoadSegment,
                change.RemoveRoadSegment,
                change.AddRoadSegmentToEuropeanRoad,
                change.RemoveRoadSegmentFromEuropeanRoad,
                change.AddRoadSegmentToNationalRoad,
                change.RemoveRoadSegmentFromNationalRoad,
                change.AddGradeSeparatedJunction,
                change.RemoveGradeSeparatedJunction
            }
            .SingleOrDefault(c => !ReferenceEquals(c, null))
            ?? throw new InvalidOperationException($"No change found in {nameof(ChangeRoadNetworkCommandItem)}.");
    }
}
