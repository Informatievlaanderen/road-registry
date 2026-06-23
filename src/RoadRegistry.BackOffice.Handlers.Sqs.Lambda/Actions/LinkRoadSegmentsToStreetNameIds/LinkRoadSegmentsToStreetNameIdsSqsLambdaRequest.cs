namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.LinkRoadSegmentsToStreetNameIds;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.SystemFlows;

public sealed record SystemLinkRoadSegmentsToStreetNameIdsSqsLambdaRequest : SqsLambdaRequest
{
    public SystemLinkRoadSegmentsToStreetNameIdsSqsLambdaRequest(string groupId, SystemLinkRoadSegmentsToStreetNameIdsSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public SystemLinkRoadSegmentsToStreetNameIdsSqsRequest Request { get; }
}
