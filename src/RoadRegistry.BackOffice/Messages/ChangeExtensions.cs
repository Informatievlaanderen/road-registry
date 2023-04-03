namespace RoadRegistry.BackOffice.Messages;

using System.Collections.Generic;
using System.Linq;

public static class ChangeExtensions
{
    public static IEnumerable<object> Flatten(this IEnumerable<RequestedChange> changes)
    {
        return changes.Select(Flatten);
    }

    public static object Flatten(this RequestedChange change)
    {
        return new object[]
            {
                change.AddRoadNode,
                change.ModifyRoadNode,
                change.RemoveRoadNode,
                change.AddRoadSegment,
                change.ModifyRoadSegment,
                change.ModifyRoadSegmentAttributes,
                change.RemoveRoadSegment,
                change.AddRoadSegmentToEuropeanRoad,
                change.RemoveRoadSegmentFromEuropeanRoad,
                change.AddRoadSegmentToNationalRoad,
                change.RemoveRoadSegmentFromNationalRoad,
                change.AddRoadSegmentToNumberedRoad,
                change.ModifyRoadSegmentOnNumberedRoad,
                change.RemoveRoadSegmentFromNumberedRoad,
                change.AddGradeSeparatedJunction,
                change.ModifyGradeSeparatedJunction,
                change.RemoveGradeSeparatedJunction
            }
            .Single(_ => !ReferenceEquals(_, null));
    }

    public static IEnumerable<object> Flatten(this IEnumerable<AcceptedChange> changes)
    {
        return changes.Select(Flatten);
    }

    public static object Flatten(this AcceptedChange change)
    {
        return new object[]
            {
                change.RoadNodeAdded,
                change.RoadNodeModified,
                change.RoadNodeRemoved,
                change.RoadSegmentAdded,
                change.RoadSegmentModified,
                change.RoadSegmentAttributesModified,
                change.RoadSegmentRemoved,
                change.RoadSegmentAddedToEuropeanRoad,
                change.RoadSegmentRemovedFromEuropeanRoad,
                change.RoadSegmentAddedToNationalRoad,
                change.RoadSegmentRemovedFromNationalRoad,
                change.RoadSegmentAddedToNumberedRoad,
                change.RoadSegmentOnNumberedRoadModified,
                change.RoadSegmentRemovedFromNumberedRoad,
                change.GradeSeparatedJunctionAdded,
                change.GradeSeparatedJunctionModified,
                change.GradeSeparatedJunctionRemoved
            }
            .Single(_ => !ReferenceEquals(_, null));
    }

    public static IEnumerable<object> Flatten(this IEnumerable<RejectedChange> changes)
    {
        return changes.Select(Flatten);
    }

    public static object Flatten(this RejectedChange change)
    {
        return new object[]
            {
                change.AddRoadNode,
                change.ModifyRoadNode,
                change.RemoveRoadNode,
                change.AddRoadSegment,
                change.ModifyRoadSegment,
                change.ModifyRoadSegmentAttributes,
                change.RemoveRoadSegment,
                change.AddRoadSegmentToEuropeanRoad,
                change.RemoveRoadSegmentFromEuropeanRoad,
                change.AddRoadSegmentToNationalRoad,
                change.RemoveRoadSegmentFromNationalRoad,
                change.AddRoadSegmentToNumberedRoad,
                change.ModifyRoadSegmentOnNumberedRoad,
                change.RemoveRoadSegmentFromNumberedRoad,
                change.AddGradeSeparatedJunction,
                change.ModifyGradeSeparatedJunction,
                change.RemoveGradeSeparatedJunction
            }
            .Single(_ => !ReferenceEquals(_, null));
    }
}
