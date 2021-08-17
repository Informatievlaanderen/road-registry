namespace RoadRegistry.BackOffice.Messages
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
            typeof(RoadNetworkChangesArchiveAccepted),
            typeof(RoadNetworkChangesArchiveRejected),
            typeof(RoadNetworkChangesArchiveUploaded),
            typeof(RoadNetworkChangesAccepted),
            typeof(RoadNetworkChangesRejected),
            typeof(RoadNetworkExtractGotRequested),
            typeof(RoadNetworkExtractDownloadBecameAvailable),
            typeof(RoadNetworkExtractChangesArchiveAccepted),
            typeof(RoadNetworkExtractChangesArchiveRejected),
            typeof(RoadNetworkExtractChangesArchiveUploaded)
        };
    }
}
