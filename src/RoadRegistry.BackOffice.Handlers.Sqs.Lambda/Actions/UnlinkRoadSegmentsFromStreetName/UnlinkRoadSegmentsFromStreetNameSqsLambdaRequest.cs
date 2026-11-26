namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.UnlinkRoadSegmentsFromStreetName;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Handlers.Sqs.SystemFlows;

public sealed record SystemUnlinkRoadSegmentsFromStreetNameSqsLambdaRequest : SqsLambdaRequest
{
    public SystemUnlinkRoadSegmentsFromStreetNameSqsLambdaRequest(string groupId, SystemUnlinkRoadSegmentsFromStreetNameSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest;
    }

    public SystemUnlinkRoadSegmentsFromStreetNameSqsRequest Request { get; }
}
