namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Extracts;
using Editor.Schema;
using Editor.Schema.Extracts;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using Sync.MunicipalityRegistry;

public class GetOverlappingTransactionZonesByNisCodeRequestHandler : EndpointRequestHandler<GetOverlappingTransactionZonesByNisCodeRequest, GetOverlappingTransactionZonesByNisCodeResponse>
{
    private readonly EditorContext _editorContext;
    private readonly MunicipalityEventConsumerContext _municipalityContext;

    public GetOverlappingTransactionZonesByNisCodeRequestHandler(
        EditorContext editorContext,
        MunicipalityEventConsumerContext municipalityContext,
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _editorContext = editorContext.ThrowIfNull();
        _municipalityContext = municipalityContext.ThrowIfNull();
    }

    private record ExtractRequestIntersection(ExtractRequestRecord ExtractRequest, Geometry Intersection);

    protected override async Task<GetOverlappingTransactionZonesByNisCodeResponse> InnerHandleAsync(GetOverlappingTransactionZonesByNisCodeRequest request, CancellationToken cancellationToken)
    {
        var municipality = await _municipalityContext.FindCurrentMunicipalityByNisCode(request.NisCode, cancellationToken);
        if (municipality?.Geometry is null)
        {
            return new GetOverlappingTransactionZonesByNisCodeResponse();
        }

        var extractRequestsQuery = _editorContext.ExtractRequests.Where(x => !x.IsInformative);
        ICollection<ExtractRequestIntersection> intersections;

        if (request.Buffer > 0)
        {
            intersections = await (
                from extractRequest in extractRequestsQuery
                let intersection = extractRequest.Contour.Intersection(municipality.Geometry.Buffer(request.Buffer))
                where intersection != null
                select new ExtractRequestIntersection(extractRequest, intersection)
            ).ToListAsync(cancellationToken);
        }
        else
        {
            intersections = await (
                from extractRequest in extractRequestsQuery
                let intersection = extractRequest.Contour.Intersection(municipality.Geometry)
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
