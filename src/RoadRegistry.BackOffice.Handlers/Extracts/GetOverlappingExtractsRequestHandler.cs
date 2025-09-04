namespace RoadRegistry.BackOffice.Handlers.Extracts;

using Abstractions;
using Abstractions.Extracts;
using Framework;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts.Schema;

public class GetOverlappingExtractsRequestHandler : EndpointRequestHandler<GetOverlappingExtractsRequest, GetOverlappingExtractsResponse>
{
    private readonly ExtractsDbContext _context;

    public GetOverlappingExtractsRequestHandler(
        ExtractsDbContext context,
        CommandHandlerDispatcher dispatcher,
        ILoggerFactory loggerFactory) : base(dispatcher, loggerFactory.CreateLogger<GetOverlappingExtractsRequestHandler>())
    {
        _context = context;
    }

    protected override async Task<GetOverlappingExtractsResponse> InnerHandleAsync(GetOverlappingExtractsRequest request, CancellationToken cancellationToken)
    {
        var downloadIds = await _context.GetOverlappingExtractDownloadIds(request.Contour, cancellationToken);

        return new GetOverlappingExtractsResponse
        {
            DownloadIds = downloadIds
        };
    }
}
