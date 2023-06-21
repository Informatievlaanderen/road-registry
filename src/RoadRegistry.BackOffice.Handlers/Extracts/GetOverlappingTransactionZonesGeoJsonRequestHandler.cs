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

public class GetOverlappingTransactionZonesGeoJsonRequestHandler : EndpointRequestHandler<GetOverlappingTransactionZonesGeoJsonRequest, GetOverlappingTransactionZonesGeoJsonResponse>
{
    private readonly EditorContext _context;

    public GetOverlappingTransactionZonesGeoJsonRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context;
    }

    public override async Task<GetOverlappingTransactionZonesGeoJsonResponse> HandleAsync(GetOverlappingTransactionZonesGeoJsonRequest request, CancellationToken cancellationToken)
    {
        var transactionZones = await _context.ExtractRequests
            .Where(x => !x.IsInformative)
            .ToListAsync(cancellationToken);

        var intersectionGeometries = (
            from t1 in transactionZones
            from t2 in transactionZones
            where t1.DownloadId != t2.DownloadId
            let intersection = t1.Contour.Intersection(t2.Contour)
            where intersection is not null && intersection != Polygon.Empty
            select intersection
        ).Distinct().ToArray();

        return new GetOverlappingTransactionZonesGeoJsonResponse
        {
            FeatureCollection = new FeatureCollection(intersectionGeometries
                .Select(geometry => new Feature(geometry.ToMultiPolygon().ToGeoJson()))
                .ToList()
            )
        };
    }
}
