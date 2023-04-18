namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNodes;

using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;

public sealed class CorrectRoadNodeVersionsSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectRoadNodeVersionsRequest>
{
    public CorrectRoadNodeVersionsRequest Request { get; init; }

    public CorrectRoadNodeVersionsSqsRequest()
    {
        Request = new CorrectRoadNodeVersionsRequest();
    }
}
