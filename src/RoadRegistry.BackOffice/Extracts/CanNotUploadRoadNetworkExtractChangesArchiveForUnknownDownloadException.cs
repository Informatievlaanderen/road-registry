namespace RoadRegistry.BackOffice.Extracts;

using System;
using System.Runtime.Serialization;

[Serializable]
public class CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException : Exception
{
    public CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException(ExternalExtractRequestId externalRequestId, ExtractRequestId requestId, DownloadId attemptedDownloadId, UploadId uploadId)
        : base("Can not upload a road network extract changes archive for the unknown download.")
    {
        ExternalRequestId = externalRequestId;
        RequestId = requestId;
        AttemptedDownloadId = attemptedDownloadId;
        UploadId = uploadId;
    }

    protected CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
    }

    public DownloadId AttemptedDownloadId { get; }
    public ExternalExtractRequestId ExternalRequestId { get; }
    public ExtractRequestId RequestId { get; }
    public UploadId UploadId { get; }
}