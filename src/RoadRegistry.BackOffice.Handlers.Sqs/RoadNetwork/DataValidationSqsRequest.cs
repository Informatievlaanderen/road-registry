namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;

[BlobRequest]
public sealed class DataValidationSqsRequest : SqsRequest
{
    public required MigrateRoadNetworkSqsRequest MigrateRoadNetworkSqsRequest { get; set; }
}
