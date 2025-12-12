namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests.RoadNetwork;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using CommandHandling.Actions.ChangeRoadNetwork;
using CommandHandling.Actions.RemoveRoadSegments;
using Sqs.RoadNetwork;

public sealed record RemoveRoadSegmentsCommandSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<RemoveRoadSegmentsCommand>
{
    public RemoveRoadSegmentsCommandSqsLambdaRequest(string groupId, RemoveRoadSegmentsSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public RemoveRoadSegmentsCommand Request { get; init; }
}
