namespace RoadRegistry.Model
{
    using System.Linq;
    using Commands;

    internal static class RoadNetworkChangeExtensions
    {
        public static object PickChange(this RoadNetworkChange change) =>
            new object[] {change.AddRoadNode, change.AddRoadNode2}.Single(_ => !ReferenceEquals(_, null));
    }
}
