namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeGradeSeparatedJunctionToGradeJunction;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.GradeSeparatedJunctions.V2;

public sealed record ChangeGradeSeparatedJunctionToGradeJunctionSqsLambdaRequest : SqsLambdaRequest
{
    public ChangeGradeSeparatedJunctionToGradeJunctionSqsLambdaRequest(string groupId, ChangeGradeSeparatedJunctionToGradeJunctionSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public ChangeGradeSeparatedJunctionToGradeJunctionSqsRequest Request { get; }
}
