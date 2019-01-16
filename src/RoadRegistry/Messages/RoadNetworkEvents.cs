namespace RoadRegistry.Messages
{
    using System;

    public static class RoadNetworkEvents
    {
        public static readonly Type[] All = {
            typeof(BeganRoadNetworkImport),
            typeof(CompletedRoadNetworkImport),
            typeof(ImportedGradeSeparatedJunction),
            typeof(ImportedRoadNode),
            typeof(ImportedRoadSegment),
            typeof(ImportedOrganization),
            typeof(RoadNetworkChangesAccepted),
            typeof(RoadNetworkChangesRejected)
        };
    }
}
