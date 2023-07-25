namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;

using Abstractions;
using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadSegments;

public sealed record ChangeRoadSegmentsDynamicAttributesSqsLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<ChangeRoadSegmentsDynamicAttributesRequest>
{
    public ChangeRoadSegmentsDynamicAttributesSqsLambdaRequest(string groupId, ChangeRoadSegmentsDynamicAttributesSqsRequest sqsRequest)
        : base(
            groupId,
            sqsRequest.TicketId,
            sqsRequest.IfMatchHeaderValue,
            sqsRequest.ProvenanceData.ToProvenance(),
            sqsRequest.Metadata)
    {
        Request = sqsRequest.Request;
    }

    public ChangeRoadSegmentsDynamicAttributesRequest Request { get; init; }
}
