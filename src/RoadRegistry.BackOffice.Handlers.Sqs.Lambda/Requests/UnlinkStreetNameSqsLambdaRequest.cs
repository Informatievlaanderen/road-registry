namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;

using Abstractions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.RoadNetwork.ValueObjects;
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
    public RoadSegmentId RoadSegmentId => new(Request.WegsegmentId);
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod => RoadSegmentGeometryDrawMethod.Parse(Request.Methode);
}
