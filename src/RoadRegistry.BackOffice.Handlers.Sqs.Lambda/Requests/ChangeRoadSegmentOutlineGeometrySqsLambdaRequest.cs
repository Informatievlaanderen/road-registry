namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;

using Abstractions;
using Abstractions.RoadSegmentsOutline;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadSegments;

public sealed record ChangeRoadSegmentOutlineGeometrySqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<ChangeRoadSegmentOutlineGeometryRequest>
{
    public ChangeRoadSegmentOutlineGeometrySqsLambdaRequest(string groupId, ChangeRoadSegmentOutlineGeometrySqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public ChangeRoadSegmentOutlineGeometryRequest Request { get; init; }
}
