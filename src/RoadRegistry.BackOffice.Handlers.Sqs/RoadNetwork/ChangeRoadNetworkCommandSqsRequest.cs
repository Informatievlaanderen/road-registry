namespace RoadRegistry.BackOffice.Handlers.Sqs.RoadNetwork;

using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.CommandHandling.Actions.ChangeRoadNetwork;

[BlobRequest]
public sealed class ChangeRoadNetworkCommandSqsRequest : SqsRequest, IHasBackOfficeRequest<ChangeRoadNetworkCommand>
{
    public ChangeRoadNetworkCommand Request { get; init; }
}
