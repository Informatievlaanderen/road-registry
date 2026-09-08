namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeGradeSeparatedJunctionAttributes;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.GradeSeparatedJunctions.V2;

public sealed record ChangeGradeSeparatedJunctionAttributesV2SqsLambdaRequest : SqsLambdaRequest
{
    public ChangeGradeSeparatedJunctionAttributesV2SqsLambdaRequest(string groupId, ChangeGradeSeparatedJunctionAttributesV2SqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public ChangeGradeSeparatedJunctionAttributesV2SqsRequest Request { get; }
}
