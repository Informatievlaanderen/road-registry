namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;

using Abstractions;
using Abstractions.RoadSegmentsOutline;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadSegments;

public sealed record DeleteRoadSegmentOutlineSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<DeleteRoadSegmentOutlineRequest>
{
    public DeleteRoadSegmentOutlineSqsLambdaRequest(string groupId, DeleteRoadSegmentOutlineSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public DeleteRoadSegmentOutlineRequest Request { get; init; }
}
