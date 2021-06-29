namespace RoadRegistry.BackOffice.Scenarios
{
    using Framework;
    using Messages;

    public static class TheExternalSystem
    {
        public static Command PutsInARoadNetworkExtractRequest(
            ExternalExtractRequestId requestId,
            DownloadId downloadId,
            RoadNetworkExtractGeometry contour)
        {
            return new Command(new RequestRoadNetworkExtract
            {
                ExternalRequestId = requestId,
                DownloadId = downloadId,
                Contour = contour
            });
        }
    }
}
