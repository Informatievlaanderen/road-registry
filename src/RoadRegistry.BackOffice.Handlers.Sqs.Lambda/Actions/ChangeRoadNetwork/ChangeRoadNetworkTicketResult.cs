namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Actions.ChangeRoadNetwork;

public sealed record ChangeRoadNetworkTicketResult
{
    public required ChangeRoadNetworkChangesTicketResult Changes { get; init; }
}

public sealed record ChangeRoadNetworkChangesTicketResult
{
    public required ChangeRoadNetworkEntityChangesTicketResult RoadNodes { get; init; }
    public required ChangeRoadNetworkEntityChangesTicketResult RoadSegments { get; init; }
    public required ChangeRoadNetworkEntityChangesTicketResult GradeSeparatedJunctions { get; init; }
}

public sealed record ChangeRoadNetworkEntityChangesTicketResult
{
    public required ICollection<int> Added { get; init; }
    public required ICollection<int> Modified { get; init; }
    public required ICollection<int> Removed { get; init; }
}
