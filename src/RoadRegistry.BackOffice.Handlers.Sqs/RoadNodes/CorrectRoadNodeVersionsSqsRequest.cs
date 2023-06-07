namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNodes;

using Abstractions;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.BackOffice.Abstractions.RoadNodes;

public sealed class CorrectRoadNodeVersionsSqsRequest : SqsRequest, IHasBackOfficeRequest<CorrectRoadNodeVersionsRequest>
{
    public CorrectRoadNodeVersionsRequest Request { get; init; }

    public CorrectRoadNodeVersionsSqsRequest()
    {
        Request = new CorrectRoadNodeVersionsRequest();
    }
}
