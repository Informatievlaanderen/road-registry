namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using Editor.Schema;
using Editor.Schema.Extracts;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

public class GetOverlappingTransactionZonesByNisCodeRequestHandler : EndpointRequestHandler<GetOverlappingTransactionZonesByNisCodeRequest, GetOverlappingTransactionZonesByNisCodeResponse>
{
    private readonly EditorContext _context;

    public GetOverlappingTransactionZonesByNisCodeRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context;
    }

    private record ExtractRequestIntersection(ExtractRequestRecord ExtractRequest, Geometry Intersection);

    public override async Task<GetOverlappingTransactionZonesByNisCodeResponse> HandleAsync(GetOverlappingTransactionZonesByNisCodeRequest request, CancellationToken cancellationToken)
    {
        var extractRequestsQuery = _context.ExtractRequests.Where(x => !x.IsInformative);

        ICollection<ExtractRequestIntersection> intersections;

        if (request.Buffer > 0)
        {
            intersections = await (
                from extractRequest in extractRequestsQuery
                let municipalityGeometry = _context.MunicipalityGeometries.SingleOrDefault(x => x.NisCode == request.NisCode)
                where municipalityGeometry != null
                let intersection = extractRequest.Contour.Intersection(municipalityGeometry.Geometry.Buffer(request.Buffer))
                where intersection != null
                select new ExtractRequestIntersection(extractRequest, intersection)
            ).ToListAsync(cancellationToken);
        }
        else
        {
            intersections = await (
                from extractRequest in extractRequestsQuery
                let municipalityGeometry = _context.MunicipalityGeometries.SingleOrDefault(x => x.NisCode == request.NisCode)
                where municipalityGeometry != null
                let intersection = extractRequest.Contour.Intersection(municipalityGeometry.Geometry)
                where intersection != null
                select new ExtractRequestIntersection(extractRequest, intersection)
            ).ToListAsync(cancellationToken);
        }

        var downloadIds = intersections
            .Where(x => (x.Intersection is Polygon polygon && polygon.Area > 0)
                        || (x.Intersection is MultiPolygon multiPolygon && multiPolygon.Area > 0))
            .Select(x => x.ExtractRequest.DownloadId)
            .ToList();

        return new GetOverlappingTransactionZonesByNisCodeResponse
        {
            DownloadIds = downloadIds.Distinct().ToList()
        };
    }
}
