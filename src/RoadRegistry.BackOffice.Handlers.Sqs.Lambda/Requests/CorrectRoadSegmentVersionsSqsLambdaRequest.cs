namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;

using Abstractions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadSegments;

public sealed record CorrectRoadSegmentVersionsSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<CorrectRoadSegmentVersionsRequest>
{
    public CorrectRoadSegmentVersionsSqsLambdaRequest(string groupId, CorrectRoadSegmentVersionsSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public CorrectRoadSegmentVersionsRequest Request { get; init; }
}
