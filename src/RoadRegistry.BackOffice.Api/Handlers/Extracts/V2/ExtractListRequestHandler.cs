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

        var extractRequestQuery = _extractsDbContext.ExtractRequests.AsQueryable();
        if (request.OrganizationCode is not null)
        {
            extractRequestQuery = extractRequestQuery.Where(er => er.OrganizationCode == request.OrganizationCode);
        }

        var query =
                from extractRequest in extractRequestQuery
                join extractDownload in _extractsDbContext.ExtractDownloads on extractRequest.CurrentDownloadId equals extractDownload.DownloadId
                where extractRequest.RequestedOn > now.AddYears(-1)
                select new { Extract = extractRequest, Download = extractDownload }
            ;

        query = query
            .OrderBy(x => x.Download.Closed ? 1 : 0)
            .ThenBy(x =>
                x.Download.UploadStatus != null && x.Download.UploadStatus != ExtractUploadStatus.Accepted
                    ? 0
                    : x.Download.UploadStatus == null
                        ? 1
                        : x.Download.UploadStatus == ExtractUploadStatus.Accepted
                            ? 2
                            : 9)
            .ThenByDescending(x => x.Download.RequestedOn);

        var records = await query.ToListAsync(cancellationToken);

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
                    DownloadStatus = record.Download.DownloadStatus.ToString(),
                    UploadStatus = record.Download.UploadStatus?.ToString(),
                    Closed = record.Download.Closed,
                })
                .ToList()
        };
    }
}
