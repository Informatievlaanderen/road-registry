namespace RoadRegistry.BackOffice.Api.Handlers.Extracts.V2;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.Extracts.Schema;

public class ExtractListRequestHandler : EndpointRequestHandler<ExtractListRequest, ExtractListResponse>
{
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IClock _clock;

    public ExtractListRequestHandler(
        ExtractsDbContext extractsDbContext,
        CommandHandlerDispatcher dispatcher,
        IClock clock,
        ILogger<ExtractListRequestHandler> logger)
        : base(dispatcher, logger)
    {
        _extractsDbContext = extractsDbContext;
        _clock = clock;
    }

    protected override async Task<ExtractListResponse> InnerHandleAsync(ExtractListRequest request, CancellationToken cancellationToken)
    {
        var now = _clock.GetCurrentInstant().ToDateTimeOffset();

        var records = await (
            from extractRequest in _extractsDbContext.ExtractRequests
            join extractDownload in _extractsDbContext.ExtractDownloads on extractRequest.CurrentDownloadId equals extractDownload.DownloadId
            where extractRequest.OrganizationCode == request.OrganizationCode
                && extractRequest.RequestedOn > now.AddYears(-1)
            select new { Extract = extractRequest, Download = extractDownload }
        ).ToListAsync(cancellationToken);

        return new ExtractListResponse
        {
            Items = records
                .Select(record => new ExtractListItem
                {
                    DownloadId = new DownloadId(record.Download.DownloadId),
                    Description = record.Extract.Description,
                    ExtractRequestId = ExtractRequestId.FromString(record.Extract.ExtractRequestId),
                    RequestedOn = record.Extract.RequestedOn,
                    IsInformative = record.Download.IsInformative,
                    ArchiveId = record.Download.ArchiveId is not null ? new ArchiveId(record.Download.ArchiveId) : null,
                    DownloadAvailable = record.Download.DownloadAvailable,
                    ExtractDownloadTimeoutOccurred = record.Download.ExtractDownloadTimeoutOccurred
                })
                .ToList()
        };
    }
}
