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

    public override async Task<GetTransactionZonesGeoJsonResponse> HandleAsync(GetTransactionZonesGeoJsonRequest request, CancellationToken cancellationToken)
    {
        var transactionZones = await _context.ExtractRequests
            .Where(x => x.IsInformative == true)
            .ToListAsync(cancellationToken);
        
        return new GetTransactionZonesGeoJsonResponse
        {
            FeatureCollection = new FeatureCollection(transactionZones
                .Select(municipality => new Feature(((MultiPolygon)municipality.Contour).ToGeoJson(), new
                {
                    municipality.Description,
                    municipality.DownloadId,
                    municipality.ExternalRequestId,
                    municipality.RequestedOn
                }))
                .ToList()
            )
        };
    }
}
