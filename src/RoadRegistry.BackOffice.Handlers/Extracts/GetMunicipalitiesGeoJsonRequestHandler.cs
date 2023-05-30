namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using BackOffice.Extensions;
using Editor.Schema;
using Framework;
using GeoJSON.Net.Feature;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

public class GetMunicipalitiesGeoJsonRequestHandler : EndpointRequestHandler<GetMunicipalitiesGeoJsonRequest, GetMunicipalitiesGeoJsonResponse>
{
    private readonly EditorContext _context;

    public GetMunicipalitiesGeoJsonRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context;
    }

    public override async Task<GetMunicipalitiesGeoJsonResponse> HandleAsync(GetMunicipalitiesGeoJsonRequest request, CancellationToken cancellationToken)
    {
        var municipalities = await _context.MunicipalityGeometries.ToListAsync(cancellationToken);

        return new GetMunicipalitiesGeoJsonResponse
        {
            FeatureCollection = new FeatureCollection(municipalities
                .Select(municipality => new Feature(((MultiPolygon)municipality.Geometry).ToGeoJson(), new
                {
                    municipality.NisCode
                }))
                .ToList()
            )
        };
    }
}
