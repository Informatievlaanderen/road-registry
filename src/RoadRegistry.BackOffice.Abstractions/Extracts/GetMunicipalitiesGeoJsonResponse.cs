namespace RoadRegistry.BackOffice.Abstractions.Extracts;

using GeoJSON.Net.Feature;

public sealed record GetMunicipalitiesGeoJsonResponse : EndpointResponse
{
    public FeatureCollection FeatureCollection { get; init; }
}
