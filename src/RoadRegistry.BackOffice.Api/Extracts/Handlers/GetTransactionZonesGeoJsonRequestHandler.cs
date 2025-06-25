namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Extracts;
using Editor.Schema;
using Extensions;
using Framework;
using GeoJSON.Net.Feature;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

public class GetTransactionZonesGeoJsonRequestHandler : EndpointRequestHandler<GetTransactionZonesGeoJsonRequest, GetTransactionZonesGeoJsonResponse>
{
    private readonly EditorContext _context;

    public GetTransactionZonesGeoJsonRequestHandler(
        EditorContext context,
        CommandHandlerDispatcher dispatcher,
        ILogger<DownloadExtractByContourRequestHandler> logger) : base(dispatcher, logger)
    {
        _context = context;
    }

    protected override async Task<GetTransactionZonesGeoJsonResponse> InnerHandleAsync(GetTransactionZonesGeoJsonRequest request, CancellationToken cancellationToken)
    {
        var transactionZones = await _context.ExtractRequests
            .Where(x => !x.IsInformative)
            .ToListAsync(cancellationToken);

        return new GetTransactionZonesGeoJsonResponse
        {
            FeatureCollection = new FeatureCollection(transactionZones
                .Select(record => new Feature(record.Contour.ToMultiPolygon().ToGeoJson(), new
                {
                    record.Description,
                    DownloadId = DownloadId.FromValue(record.DownloadId).ToString(),
                    record.ExternalRequestId,
                    record.RequestedOn
                }))
                .ToList()
            )
        };
    }
}
