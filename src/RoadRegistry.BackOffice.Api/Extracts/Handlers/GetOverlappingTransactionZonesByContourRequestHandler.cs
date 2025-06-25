namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Extracts;
using Editor.Schema;
using Framework;
using Microsoft.Extensions.Logging;
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

    protected override async Task<GetOverlappingTransactionZonesByContourResponse> InnerHandleAsync(GetOverlappingTransactionZonesByContourRequest request, CancellationToken cancellationToken)
    {
        var geometry = new WKTReader().Read(request.Contour);

        var downloadIds = await _context.GetOverlappingExtractDownloadIds(geometry, null, cancellationToken);

        return new GetOverlappingTransactionZonesByContourResponse
        {
            DownloadIds = downloadIds
        };
    }
}
