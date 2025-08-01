namespace RoadRegistry.BackOffice.Api.Handlers.Extracts;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Extracts.V2;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
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

        var records = await _extractsDbContext.ExtractRequests
            .Where(x => x.OrganizationCode == request.OrganizationCode)
            .Where(x => x.RequestedOn > now.AddYears(-1))
            .OrderByDescending(x => x.RequestedOn)
            .ToListAsync(cancellationToken);

        return new ExtractListResponse
        {
            Items = records
                .Select(record => new ExtractListItem
                {
                    DownloadId = new DownloadId(record.DownloadId),
                    Description = record.Description,
                    ExtractRequestId = ExtractRequestId.FromString(record.ExtractRequestId),
                    RequestedOn = record.RequestedOn,
                    IsInformative = record.IsInformative,
                    ArchiveId = record.ArchiveId is not null ? new ArchiveId(record.ArchiveId) : null,
                    DownloadAvailable = record.DownloadAvailable,
                    ExtractDownloadTimeoutOccurred = record.ExtractDownloadTimeoutOccurred
                })
                .ToList()
        };
    }
}
