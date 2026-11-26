namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RelinkRoadSegmentsToStreetName;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.SystemFlows;

public sealed record SystemRelinkRoadSegmentsToStreetNameSqsLambdaRequest : SqsLambdaRequest
{
    public SystemRelinkRoadSegmentsToStreetNameSqsLambdaRequest(string groupId, SystemRelinkRoadSegmentsToStreetNameSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public SystemRelinkRoadSegmentsToStreetNameSqsRequest Request { get; }
}
