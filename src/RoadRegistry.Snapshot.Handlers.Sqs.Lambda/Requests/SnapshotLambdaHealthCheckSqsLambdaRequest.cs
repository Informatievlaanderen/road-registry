namespace RoadRegistry.Snapshot.Handlers.Sqs.Lambda.Requests;

using BackOffice;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.Snapshot.Handlers.Sqs.Infrastructure;

public sealed record SnapshotLambdaHealthCheckSqsLambdaRequest : SqsLambdaRequest
{
    public SnapshotLambdaHealthCheckSqsLambdaRequest(string groupId, SnapshotLambdaHealthCheckSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            new RoadRegistryProvenanceData().ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public SnapshotLambdaHealthCheckSqsRequest Request { get; init; }
}
