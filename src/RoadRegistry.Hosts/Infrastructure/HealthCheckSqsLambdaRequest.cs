namespace RoadRegistry.Hosts.Infrastructure;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;

public sealed record HealthCheckSqsLambdaRequest : SqsLambdaRequest
{
    public HealthCheckSqsLambdaRequest(string groupId, HealthCheckSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            default,
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public HealthCheckSqsRequest Request { get; init; }
}
