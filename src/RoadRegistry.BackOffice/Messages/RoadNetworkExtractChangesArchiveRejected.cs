namespace RoadRegistry.BackOffice.Messages
{
    using System;
    using Be.Vlaanderen.Basisregisters.EventHandling;

    [EventName("RoadNetworkExtractChangesArchiveRejected")]
    [EventDescription("Indicates the road network extract changes archive was rejected.")]
    public class RoadNetworkExtractChangesArchiveRejected
    {
        public string RequestId { get; set; }
        public string ExternalRequestId { get; set; }
        public Guid DownloadId { get; set; }
        public Guid UploadId { get; set; }
        public string ArchiveId { get; set; }
        public FileProblem[] Problems { get; set; }
        public string When { get; set; }
    }
}
