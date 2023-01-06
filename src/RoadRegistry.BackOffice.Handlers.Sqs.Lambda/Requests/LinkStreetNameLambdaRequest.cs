namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Requests;

using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Requests;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.RoadSegments;
using RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

public sealed record LinkStreetNameLambdaRequest :
    SqsLambdaRequest,
    IHasBackOfficeRequest<LinkStreetNameRequest>
{
    public LinkStreetNameLambdaRequest(string groupId, LinkStreetNameSqsRequest sqsRequest)
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
}
