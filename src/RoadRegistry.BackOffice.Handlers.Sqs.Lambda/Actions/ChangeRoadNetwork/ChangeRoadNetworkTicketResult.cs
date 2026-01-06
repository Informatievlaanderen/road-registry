namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;

using RoadRegistry.RoadNetwork.Events.V2;

public sealed record ChangeRoadNetworkTicketResult
{
    public required RoadNetworkChangedSummary Summary { get; init; }
}
