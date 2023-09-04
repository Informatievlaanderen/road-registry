namespace RoadRegistry.BackOffice
{
    public static class WellKnownEventProcessorNames
    {
        public const string RoadNetwork = "RoadNetworkEventProcessor";
        public const string ExtractRequest = "ExtractRequestEventProcessor";
        public const string ExtractDownload = "ExtractDownloadEventProcessor";
        public const string ExtractUpload = "ExtractUploadEventProcessor";
        public const string Organization = "OrganizationEventProcessor";
        public const string ChangeFeed = "ChangeFeedEventProcessor";
        public const string Municipality = "MunicipalityEventProcessor";
    }
}
