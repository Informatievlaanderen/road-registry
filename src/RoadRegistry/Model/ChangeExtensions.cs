namespace RoadRegistry.Model
{
    using System.Collections.Generic;
    using System.Linq;
    using Messages;

    internal static class ChangeExtensions
    {
        public static IEnumerable<object> Flatten(this IEnumerable<RequestedChange> changes) =>
            changes.Select(change => change.Flatten());

        private static object Flatten(this RequestedChange change) =>
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

        private static object Flatten(this Messages.AcceptedChange change) =>
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
