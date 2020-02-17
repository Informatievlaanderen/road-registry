namespace RoadRegistry.BackOffice.Messages
{
    using System;

    public static class RoadNetworkCommands
    {
        public static readonly Type[] All = {
            typeof(UploadRoadNetworkChangesArchive),
            typeof(ChangeRoadNetwork),
            typeof(ChangeRoadNetworkBasedOnArchive)
        };
    }
}
