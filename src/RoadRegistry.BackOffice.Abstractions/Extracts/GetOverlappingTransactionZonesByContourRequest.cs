namespace RoadRegistry.BackOffice.Abstractions.Extracts;

public sealed record GetOverlappingTransactionZonesByContourRequest : EndpointRequest<GetOverlappingTransactionZonesByContourResponse>
{
    public string Contour { get; init; }
}
