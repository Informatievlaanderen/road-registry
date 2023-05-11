namespace RoadRegistry.BackOffice.Messages;

using System;

public class UploadRoadNetworkExtractChangesArchive
{
    public string ArchiveId { get; set; }
    public Guid DownloadId { get; set; }
    public string RequestId { get; set; }
    public Guid UploadId { get; set; }
    public bool FeatureCompareCompleted { get; set; }
    public bool UseZipArchiveFeatureCompareTranslator { get; set; }
}
