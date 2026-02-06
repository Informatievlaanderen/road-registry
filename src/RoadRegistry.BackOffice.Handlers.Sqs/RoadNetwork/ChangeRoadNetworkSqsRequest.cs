namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;

[BlobRequest]
public sealed class ChangeRoadNetworkSqsRequest : SqsRequest
{
    public required ICollection<ChangeRoadNetworkItem> Changes { get; set; } = [];
    public required DownloadId DownloadId { get; set; }
    public required UploadId UploadId { get; set; }
    public bool SendFailedEmail { get; set; }
}
