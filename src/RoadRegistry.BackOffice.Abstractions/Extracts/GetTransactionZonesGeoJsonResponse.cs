namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using GeoJSON.Net.Feature;

public sealed record GetTransactionZonesGeoJsonResponse : EndpointResponse
{
    public FeatureCollection FeatureCollection { get; init; }
}
