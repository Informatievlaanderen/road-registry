namespace RoadRegistry.BackOffice
{
    public static class WellKnownEventProcessorNames
    {
        public static string RoadNetwork = "RoadNetworkEventProcessor";
        public static string ExtractRequest = "ExtractRequestEventProcessor";
        public static string ExtractDownload = "ExtractDownloadEventProcessor";
        public static string ExtractUpload = "ExtractUploadEventProcessor";
        public static string Organization = "OrganizationEventProcessor";
        public static string ChangeFeed = "ChangeFeedEventProcessor";
        public static string Municipality = "MunicipalityEventProcessor";
    }
}
