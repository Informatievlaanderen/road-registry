namespace RoadRegistry.BackOffice.Extracts
{
    using System;

    public class CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce : Exception
    {
        public ExternalExtractRequestId ExternalRequestId { get; }
        public ExtractRequestId RequestId { get; }
        public DownloadId DownloadId { get; }
        public UploadId UploadId { get; }

        public CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnce(ExternalExtractRequestId externalRequestId, ExtractRequestId requestId, DownloadId downloadId, UploadId uploadId)
            : base("Can not upload a road network extract changes archive for the same download more than once.")
        {
            ExternalRequestId = externalRequestId;
            RequestId = requestId;
            DownloadId = downloadId;
            UploadId = uploadId;
        }
    }
}
