namespace RoadRegistry.BackOffice.Extracts;

using System;

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

    public DownloadId AttemptedDownloadId { get; }
    public ExternalExtractRequestId ExternalRequestId { get; }
    public ExtractRequestId RequestId { get; }
    public UploadId UploadId { get; }
}
