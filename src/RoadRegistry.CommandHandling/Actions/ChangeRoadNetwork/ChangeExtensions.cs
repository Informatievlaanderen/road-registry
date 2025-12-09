namespace RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

using System.Collections.Generic;
using System.Linq;
using RoadNetwork;

public static class ChangeExtensions
{
    public static IEnumerable<IRoadNetworkChange> Flatten(this IEnumerable<ChangeRoadNetworkCommandItem> changes)
    {
        return changes.Select(Flatten);
    }

    private static IRoadNetworkChange Flatten(this ChangeRoadNetworkCommandItem change)
    {
        return new IRoadNetworkChange[]
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
                change.ModifyGradeSeparatedJunction,
                change.RemoveGradeSeparatedJunction
            }
            .SingleOrDefault(c => !ReferenceEquals(c, null))
            ?? throw new InvalidOperationException($"No change found in {nameof(ChangeRoadNetworkCommandItem)}.");
    }
}
