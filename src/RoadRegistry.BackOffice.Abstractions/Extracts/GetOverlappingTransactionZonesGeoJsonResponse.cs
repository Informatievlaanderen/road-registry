namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using GeoJSON.Net.Feature;

public sealed record GetOverlappingTransactionZonesGeoJsonResponse : EndpointResponse
{
    public FeatureCollection FeatureCollection { get; init; }
}
