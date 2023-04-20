namespace RoadRegistry.BackOffice.Abstractions.Uploads;

public interface IUploadExtractFeatureCompareRequest
{
    public string DownloadId { get; }
    public UploadExtractArchiveRequest Archive { get; }
}
