namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.DataValidation;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

public sealed record DataValidationSqsLambdaRequest : SqsLambdaRequest
{
    public DataValidationSqsLambdaRequest(string groupId, DataValidationSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public DataValidationSqsRequest Request { get; }
}
