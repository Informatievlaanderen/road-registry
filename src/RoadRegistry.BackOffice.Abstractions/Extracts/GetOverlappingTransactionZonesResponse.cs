namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record GetOverlappingTransactionZonesResponse : EndpointResponse
{
    public List<object> TransactionZones { get; init; }
}
