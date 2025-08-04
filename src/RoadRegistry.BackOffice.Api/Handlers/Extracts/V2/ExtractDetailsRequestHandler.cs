namespace RoadRegistry.BackOffice.Api.Handlers.Extracts.V2;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Framework;
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
        var record = await (
            from extractDownload in _extractsDbContext.ExtractDownloads
            join extractRequest in _extractsDbContext.ExtractRequests on extractDownload.ExtractRequestId equals extractRequest.ExtractRequestId
            where extractDownload.DownloadId == request.DownloadId.ToGuid()
            select new { Download = extractDownload, extractRequest.Description }
        ).SingleOrDefaultAsync(cancellationToken);
        if (record is null)
        {
            throw new ExtractRequestNotFoundException(request.DownloadId);
        }

        return new ExtractDetailsResponse
        {
            DownloadId = new DownloadId(record.Download.DownloadId),
            Description = record.Description,
            Contour = record.Download.Contour.ToMultiPolygon(),
            ExtractRequestId = ExtractRequestId.FromString(record.Download.ExtractRequestId),
            RequestedOn = record.Download.RequestedOn,
            IsInformative = record.Download.IsInformative,
            ArchiveId = record.Download.ArchiveId is not null ? new ArchiveId(record.Download.ArchiveId) : null,
            TicketId = TicketId.FromValue(record.Download.TicketId),
            DownloadAvailable = record.Download.DownloadAvailable,
            ExtractDownloadTimeoutOccurred = record.Download.ExtractDownloadTimeoutOccurred
        };
    }
}
