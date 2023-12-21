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
        var overlaps = await _context.ExtractRequestOverlaps
            .ToListAsync(cancellationToken);

        return new GetOverlappingTransactionZonesGeoJsonResponse
        {
            FeatureCollection = new FeatureCollection(overlaps
                .Select(overlap => new Feature(overlap.Contour.ToMultiPolygon().ToGeoJson(), new Dictionary<string, object>
                {
                    {"DownloadId1", DownloadId.FromValue(overlap.DownloadId1).ToString()},
                    {"DownloadId2", DownloadId.FromValue(overlap.DownloadId2).ToString()},
                    {"Description1", overlap.Description1},
                    {"Description2", overlap.Description2}
                }))
                .ToList()
            )
        };
    }
}
