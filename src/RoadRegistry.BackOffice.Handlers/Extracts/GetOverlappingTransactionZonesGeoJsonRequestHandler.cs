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
using RoadRegistry.Extensions;

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

    protected override async Task<GetOverlappingTransactionZonesGeoJsonResponse> InnerHandleAsync(GetOverlappingTransactionZonesGeoJsonRequest request, CancellationToken cancellationToken)
    {
        var availableOverlaps = await (
            from overlap in _context.ExtractRequestOverlaps
            join download1 in _context.ExtractDownloads on overlap.DownloadId1 equals download1.DownloadId
            join download2 in _context.ExtractDownloads on overlap.DownloadId2 equals download2.DownloadId
            where !download1.IsInformative && !download2.IsInformative
                && download1.Available && download2.Available
            select overlap
        ).ToListAsync(cancellationToken);

        return new GetOverlappingTransactionZonesGeoJsonResponse
        {
            FeatureCollection = new FeatureCollection(availableOverlaps
                .Select(overlap => new Feature(overlap.Contour.ToMultiPolygon().ToGeoJson(), new
                {
                    DownloadId1 = DownloadId.FromValue(overlap.DownloadId1).ToString(),
                    DownloadId2 = DownloadId.FromValue(overlap.DownloadId2).ToString(),
                    Description1 = overlap.Description1,
                    Description2 = overlap.Description2
                }))
                .ToList()
            )
        };
    }
}
