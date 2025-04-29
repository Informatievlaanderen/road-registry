namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.Infrastructure;
using Sqs;

public sealed record BackOfficeLambdaHealthCheckSqsLambdaRequest : SqsLambdaRequest
{
    public BackOfficeLambdaHealthCheckSqsLambdaRequest(string groupId, BackOfficeLambdaHealthCheckSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            new RoadRegistryProvenanceData().ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public BackOfficeLambdaHealthCheckSqsRequest Request { get; init; }
}
