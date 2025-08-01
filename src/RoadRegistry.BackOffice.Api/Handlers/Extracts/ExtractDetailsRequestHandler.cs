namespace RoadRegistry.BackOffice.Api.Handlers.Extracts;

using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Extracts.V2;
using Exceptions;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.Schema;

public class ExtractDetailsRequestHandler : EndpointRequestHandler<ExtractDetailsRequest, ExtractDetailsResponse>
{
    private readonly ExtractsDbContext _extractsDbContext;

    public ExtractDetailsRequestHandler(
        ExtractsDbContext extractsDbContext,
        CommandHandlerDispatcher dispatcher,
        ILogger<ExtractDetailsRequestHandler> logger)
        : base(dispatcher, logger)
    {
        _extractsDbContext = extractsDbContext;
    }

    protected override async Task<ExtractDetailsResponse> InnerHandleAsync(ExtractDetailsRequest request, CancellationToken cancellationToken)
    {
        var record = await _extractsDbContext.ExtractRequests.SingleOrDefaultAsync(x => x.DownloadId == request.DownloadId.ToGuid(), cancellationToken);
        if (record is null)
        {
            throw new ExtractRequestNotFoundException(request.DownloadId, 0);
        }

        return new ExtractDetailsResponse
        {
            DownloadId = new DownloadId(record.DownloadId),
            Description = record.Description,
            Contour = record.Contour.ToMultiPolygon(),
            ExtractRequestId = ExtractRequestId.FromString(record.ExtractRequestId),
            RequestedOn = record.RequestedOn,
            IsInformative = record.IsInformative,
            ArchiveId = record.ArchiveId is not null ? new ArchiveId(record.ArchiveId) : null,
            TicketId = TicketId.FromValue(record.TicketId),
            DownloadAvailable = record.DownloadAvailable,
            ExtractDownloadTimeoutOccurred = record.ExtractDownloadTimeoutOccurred
        };
    }
}
