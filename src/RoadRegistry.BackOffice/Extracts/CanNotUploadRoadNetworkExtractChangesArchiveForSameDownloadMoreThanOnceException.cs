namespace RoadRegistry.BackOffice.Extracts;

using System;
using System.Runtime.Serialization;

[Serializable]
public class CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException : Exception
{
    public CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException(ExternalExtractRequestId externalRequestId, ExtractRequestId requestId, DownloadId downloadId, UploadId uploadId)
        : base("Can not upload a road network extract changes archive for the same download more than once.")
    {
        ExternalRequestId = externalRequestId;
        RequestId = requestId;
        DownloadId = downloadId;
        UploadId = uploadId;
    }

    protected CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException(SerializationInfo serializationInfo, StreamingContext streamingContext)
        : base(serializationInfo, streamingContext)
    {
    }

    public ExternalExtractRequestId ExternalRequestId { get; }
    public ExtractRequestId RequestId { get; }
    public DownloadId DownloadId { get; }
    public UploadId UploadId { get; }
}
