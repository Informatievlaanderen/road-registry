namespace RoadRegistry.Events
{
    using System;

    public static class RoadNetworkEvents
    {
        public static readonly Type[] All = {
            typeof(ImportedGradeSeparatedJunction),
            typeof(ImportedReferencePoint),
            typeof(ImportedRoadNode),
            typeof(ImportedRoadSegment),
            typeof(ImportedOrganization)
        };
    }
}