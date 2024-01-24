namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadSegments;

using Abstractions.RoadSegments;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using Abstractions;

public sealed class MigrateOutlinedRoadSegmentsOutOfRoadNetworkSqsRequest : SqsRequest, IHasBackOfficeRequest<MigrateOutlinedRoadSegmentsOutOfRoadNetworkRequest>
{
    public MigrateOutlinedRoadSegmentsOutOfRoadNetworkRequest Request { get; init; }

    public MigrateOutlinedRoadSegmentsOutOfRoadNetworkSqsRequest()
    {
        Request = new MigrateOutlinedRoadSegmentsOutOfRoadNetworkRequest();
    }
}
