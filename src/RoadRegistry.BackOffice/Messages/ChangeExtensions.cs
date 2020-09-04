namespace RoadRegistry.BackOffice.Messages
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ChangeExtensions
    {
        public static IEnumerable<object> Flatten(this IEnumerable<RequestedChange> changes) =>
            changes.Select(Flatten);

        public static object Flatten(this RequestedChange change) =>
            new object[]
                {
                    change.AddRoadNode,
                    change.ModifyRoadNode,
                    change.AddRoadSegment,
                    change.ModifyRoadSegment,
                    change.AddRoadSegmentToEuropeanRoad,
                    change.AddRoadSegmentToNationalRoad,
                    change.AddRoadSegmentToNumberedRoad,
                    change.AddGradeSeparatedJunction,
                    change.ModifyGradeSeparatedJunction
                }
                .Single(_ => !ReferenceEquals(_, null));

        public static IEnumerable<object> Flatten(this IEnumerable<AcceptedChange> changes) =>
            changes.Select(Flatten);

        public static object Flatten(this AcceptedChange change) =>
            new object[]
                {
                    change.RoadNodeAdded,
                    change.RoadNodeModified,
                    change.RoadSegmentAdded,
                    change.RoadSegmentModified,
                    change.RoadSegmentAddedToEuropeanRoad,
                    change.RoadSegmentAddedToNationalRoad,
                    change.RoadSegmentAddedToNumberedRoad,
                    change.GradeSeparatedJunctionAdded,
                    change.GradeSeparatedJunctionModified
                }
                .Single(_ => !ReferenceEquals(_, null));

        public static IEnumerable<object> Flatten(this IEnumerable<RejectedChange> changes) =>
            changes.Select(Flatten);

        public static object Flatten(this RejectedChange change) =>
            new object[]
                {
                    change.AddRoadNode,
                    change.ModifyRoadNode,
                    change.AddRoadSegment,
                    change.ModifyRoadSegment,
                    change.AddRoadSegmentToEuropeanRoad,
                    change.AddRoadSegmentToNationalRoad,
                    change.AddRoadSegmentToNumberedRoad,
                    change.AddGradeSeparatedJunction,
                    change.ModifyGradeSeparatedJunction
                }
                .Single(_ => !ReferenceEquals(_, null));
    }
}
