namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;

using ScopedRoadNetwork.Events.V2;

public sealed record ChangeRoadNetworkTicketResult
{
    public required RoadNetworkChangedSummary Summary { get; init; }
}
