namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using BackOffice.Extensions;
using Editor.Schema;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

public class GetOverlappingTransactionZonesRequestHandler : EndpointRequestHandler<GetOverlappingTransactionZonesRequest, GetOverlappingTransactionZonesResponse>
{
    private readonly EditorContext _context;

    public GetOverlappingTransactionZonesRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context;
    }

    public override async Task<GetOverlappingTransactionZonesResponse> HandleAsync(GetOverlappingTransactionZonesRequest request, CancellationToken cancellationToken)
    {
        //TODO-rik find non-informative extracts adv niscode of contour

        var availableOverlaps = await (
            from overlap in _context.ExtractRequestOverlaps
            join download1 in _context.ExtractDownloads on overlap.DownloadId1 equals download1.DownloadId
            join download2 in _context.ExtractDownloads on overlap.DownloadId2 equals download2.DownloadId
            where !download1.IsInformative && !download2.IsInformative
                && download1.Available && download2.Available
            select overlap
        ).ToListAsync(cancellationToken);

        return new GetOverlappingTransactionZonesResponse
        {
            //TODO-rik temp
            // FeatureCollection = new FeatureCollection(availableOverlaps
            //     .Select(overlap => new Feature(overlap.Contour.ToMultiPolygon().To(), new
            //     {
            //         DownloadId1 = DownloadId.FromValue(overlap.DownloadId1).ToString(),
            //         DownloadId2 = DownloadId.FromValue(overlap.DownloadId2).ToString(),
            //         Description1 = overlap.Description1,
            //         Description2 = overlap.Description2
            //     }))
            //     .ToList()
            // )
        };
    }
}
