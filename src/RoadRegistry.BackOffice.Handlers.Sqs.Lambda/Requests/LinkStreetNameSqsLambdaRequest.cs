namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;

using Abstractions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadSegments;

public sealed record LinkStreetNameSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<LinkStreetNameRequest>,
    IHasRoadSegmentId
{
    public LinkStreetNameSqsLambdaRequest(string groupId, LinkStreetNameSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public LinkStreetNameRequest Request { get; init; }
    public RoadSegmentId RoadSegmentId => new(Request.WegsegmentId);
    public RoadSegmentGeometryDrawMethod GeometryDrawMethod => RoadSegmentGeometryDrawMethod.Parse(Request.Methode);
}
