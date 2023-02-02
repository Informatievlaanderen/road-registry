namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;

using Abstractions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Abstractions.RoadSegmentsOutline;
using RoadSegments;

public sealed record CreateRoadSegmentOutlineSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<CreateRoadSegmentOutlineRequest>
{
    public CreateRoadSegmentOutlineSqsLambdaRequest(string groupId, CreateRoadSegmentOutlineSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public CreateRoadSegmentOutlineRequest Request { get; init; }
}
