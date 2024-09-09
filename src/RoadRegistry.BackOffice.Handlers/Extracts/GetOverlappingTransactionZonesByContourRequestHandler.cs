namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using Editor.Schema;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

public class GetOverlappingTransactionZonesByContourRequestHandler : EndpointRequestHandler<GetOverlappingTransactionZonesByContourRequest, GetOverlappingTransactionZonesByContourResponse>
{
    private readonly EditorContext _context;

    public GetOverlappingTransactionZonesByContourRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context;
    }

    public override async Task<GetOverlappingTransactionZonesByContourResponse> HandleAsync(GetOverlappingTransactionZonesByContourRequest request, CancellationToken cancellationToken)
    {
        var geometry = new WKTReader().Read(request.Contour);

        var extractRequestsQuery = _context.ExtractRequests.Where(x => !x.IsInformative);

        var overlaps = await (
            from extractRequest in extractRequestsQuery
            let intersection = extractRequest.Contour.Intersection(geometry)
            where intersection != null
            select new { overlap = extractRequest, intersection }
        ).ToListAsync(cancellationToken);

        var downloadIds = overlaps
            .Where(x => (x.intersection is Polygon polygon && polygon.Area > 0)
                        || (x.intersection is MultiPolygon multiPolygon && multiPolygon.Area > 0))
            .Select(x => x.overlap.DownloadId)
            .Distinct()
            .ToList();

        return new GetOverlappingTransactionZonesByContourResponse
        {
            DownloadIds = downloadIds
        };
    }
}
