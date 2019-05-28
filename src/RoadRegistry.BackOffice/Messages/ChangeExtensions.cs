namespace RoadRegistry.BackOffice.Messages
{
    using System.Collections.Generic;
    using System.Linq;

    public static class ChangeExtensions
    {
        public static IEnumerable<object> Flatten(this IEnumerable<RequestedChange> changes) =>
            changes.Select(change => Flatten((RequestedChange) change));

        public static object Flatten(this RequestedChange change) =>
            new object[]
                {
                    change.AddRoadNode,
                    change.AddRoadSegment,
                    change.AddRoadSegmentToEuropeanRoad,
                    change.AddRoadSegmentToNationalRoad,
                    change.AddRoadSegmentToNumberedRoad,
                    change.AddGradeSeparatedJunction
                }
                .Single(_ => !ReferenceEquals(_, null));

        public static IEnumerable<object> Flatten(this IEnumerable<Messages.AcceptedChange> changes) =>
            changes.Select(change => change.Flatten());

        public static object Flatten(this Messages.AcceptedChange change) =>
            new object[]
                {
                    change.RoadNodeAdded,
                    change.RoadSegmentAdded,
                    change.RoadSegmentAddedToEuropeanRoad,
                    change.RoadSegmentAddedToNationalRoad,
                    change.RoadSegmentAddedToNumberedRoad,
                    change.GradeSeparatedJunctionAdded
                }
                .Single(_ => !ReferenceEquals(_, null));
    }
}
