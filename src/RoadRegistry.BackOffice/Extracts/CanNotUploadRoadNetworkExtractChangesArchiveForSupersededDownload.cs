namespace RoadRegistry.BackOffice.Extracts
{
    using System;

    public class CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload : Exception
    {
        public ExternalExtractRequestId ExternalRequestId { get; }
        public ExtractRequestId RequestId { get; }
        public DownloadId AttemptedDownloadId { get; }
        public DownloadId RequiredDownloadId { get; }
        public UploadId UploadId { get; }

        public CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownload(ExternalExtractRequestId externalRequestId, ExtractRequestId requestId, DownloadId attemptedDownloadId, DownloadId requiredDownloadId, UploadId uploadId)
            : base("Can not upload a road network extract changes archive for the attempted download because it has been superseded by the required download.")
        {
            ExternalRequestId = externalRequestId;
            RequestId = requestId;
            AttemptedDownloadId = attemptedDownloadId;
            RequiredDownloadId = requiredDownloadId;
            UploadId = uploadId;
        }
    }
}
