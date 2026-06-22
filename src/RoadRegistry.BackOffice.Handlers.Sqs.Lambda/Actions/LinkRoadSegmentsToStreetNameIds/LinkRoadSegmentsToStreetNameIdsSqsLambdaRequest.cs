namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.LinkRoadSegmentsToStreetNameIds;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadNetwork;

public sealed record LinkRoadSegmentsToStreetNameIdsSqsLambdaRequest : SqsLambdaRequest
{
    public LinkRoadSegmentsToStreetNameIdsSqsLambdaRequest(string groupId, LinkRoadSegmentsToStreetNameIdsSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public LinkRoadSegmentsToStreetNameIdsSqsRequest Request { get; }
}
