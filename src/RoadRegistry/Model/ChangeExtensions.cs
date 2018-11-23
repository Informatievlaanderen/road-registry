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
            new object[] {
                change.AddRoadNode,
                change.AddRoadSegment
            }
            .Single(_ => !ReferenceEquals(_, null));

        public static IEnumerable<object> Flatten(this IEnumerable<AcceptedChange> changes) =>
            changes.Select(change => change.Flatten());

        private static object Flatten(this AcceptedChange change) =>
            new object[] {
                    change.RoadNodeAdded,
                    change.RoadSegmentAdded
                }
                .Single(_ => !ReferenceEquals(_, null));
    }
}
