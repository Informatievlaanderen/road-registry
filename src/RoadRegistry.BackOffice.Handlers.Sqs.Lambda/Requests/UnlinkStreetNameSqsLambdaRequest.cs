namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;

using Abstractions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadSegments;

public sealed record UnlinkStreetNameSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<UnlinkStreetNameRequest>,
    IHasRoadSegmentId
{
    public UnlinkStreetNameSqsLambdaRequest(string groupId, UnlinkStreetNameSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public UnlinkStreetNameRequest Request { get; init; }
    public int RoadSegmentId => Request.WegsegmentId;
}
