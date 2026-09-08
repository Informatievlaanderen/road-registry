namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeGradeJunctionToGradeSeparatedJunction;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.GradeJunctions.V2;

public sealed record ChangeGradeJunctionToGradeSeparatedJunctionSqsLambdaRequest : SqsLambdaRequest
{
    public ChangeGradeJunctionToGradeSeparatedJunctionSqsLambdaRequest(string groupId, ChangeGradeJunctionToGradeSeparatedJunctionSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public ChangeGradeJunctionToGradeSeparatedJunctionSqsRequest Request { get; }
}
