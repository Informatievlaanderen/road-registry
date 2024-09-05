namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using BackOffice.Extensions;
using Editor.Schema;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;

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

        var overlapQuery = (
            from extractRequest in _context.ExtractRequests
            where !extractRequest.IsInformative
            select extractRequest
        );

        if (request.NisCode is not null)
        {
            if (request.Buffer > 0)
            {
                overlapQuery = (
                    from overlap in overlapQuery
                    let municipalityGeometry = _context.MunicipalityGeometries.SingleOrDefault(x => x.NisCode == request.NisCode)
                    where municipalityGeometry != null && overlap.Contour.Intersects(municipalityGeometry.Geometry.Buffer(request.Buffer))
                    select overlap
                );
            }
            else
            {
                overlapQuery = (
                    from overlap in overlapQuery
                    let municipalityGeometry = _context.MunicipalityGeometries.SingleOrDefault(x => x.NisCode == request.NisCode)
                    where municipalityGeometry != null && overlap.Contour.Intersects(municipalityGeometry.Geometry)
                    select overlap
                );
            }
        }

        if (request.Contour is not null)
        {
            var geometry = new WKTReader().Read(request.Contour);
            overlapQuery = overlapQuery.Where(x => x.Contour.Intersects(geometry));
        }

        var availableOverlaps = await overlapQuery.ToListAsync(cancellationToken);

        return new GetOverlappingTransactionZonesResponse
        {
            DownloadIds = availableOverlaps
                .Select(x => x.DownloadId)
                .Distinct()
                .ToList()
        };
    }
}
