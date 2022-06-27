namespace RoadRegistry.BackOffice.Extracts
{
    using System;

    [Serializable]
    public class CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException : Exception
    {
        public ExternalExtractRequestId ExternalRequestId { get; }
        public ExtractRequestId RequestId { get; }
        public DownloadId AttemptedDownloadId { get; }
        public UploadId UploadId { get; }

        public CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException(ExternalExtractRequestId externalRequestId, ExtractRequestId requestId, DownloadId attemptedDownloadId, UploadId uploadId)
            : base("Can not upload a road network extract changes archive for the unknown download.")
        {
            ExternalRequestId = externalRequestId;
            RequestId = requestId;
            AttemptedDownloadId = attemptedDownloadId;
            UploadId = uploadId;
        }

        protected CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException(System.Runtime.Serialization.SerializationInfo serializationInfo, System.Runtime.Serialization.StreamingContext streamingContext)
            : base(serializationInfo, streamingContext)
        { }
    }
}
