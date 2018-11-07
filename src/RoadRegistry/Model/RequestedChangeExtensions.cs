namespace RoadRegistry.Model
{
    using System.Linq;
    using Commands;

    internal static class RequestedChangeExtensions
    {
        public static object PickChange(this RequestedChange change) =>
            new object[] {
                change.AddRoadNode, 
                change.AddRoadSegment
            }
            .Single(_ => !ReferenceEquals(_, null));
    }
}
