namespace RoadRegistry.BackOffice.Messages
{
    using System;

    public class UploadRoadNetworkExtractChangesArchive
    {
        public string RequestId { get; set; }
        public Guid DownloadId { get; set; }
        public Guid UploadId { get; set; }
        public string ArchiveId { get; set; }
    }
}
