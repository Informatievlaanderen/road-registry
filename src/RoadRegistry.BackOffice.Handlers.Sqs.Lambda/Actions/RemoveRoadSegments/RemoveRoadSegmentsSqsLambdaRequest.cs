namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.RemoveRoadSegments;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;
using RoadRegistry.CommandHandling.Actions.RemoveRoadSegments;

public sealed record RemoveRoadSegmentsSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<RemoveRoadSegmentsCommand>
{
    public RemoveRoadSegmentsSqsLambdaRequest(string groupId, RemoveRoadSegmentsSqsRequest sqsRequest)
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
