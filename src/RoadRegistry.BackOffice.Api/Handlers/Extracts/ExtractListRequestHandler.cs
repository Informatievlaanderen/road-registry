namespace RoadRegistry.BackOffice.Api.Handlers.Extracts;

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
                join extractUpload in _extractsDbContext.ExtractUploads
                    on extractDownload.LatestUploadId equals extractUpload.UploadId
                    into extractUploads
                from extractUpload in extractUploads.DefaultIfEmpty()
                where extractDownload.RequestedOn > now.AddYears(-1)
                select new { Extract = extractRequest, Download = extractDownload, Upload = extractUpload }
            ;

        query = query
            .OrderBy(x => x.Download.Closed
                ? 9
                : x.Upload.Status == ExtractUploadStatus.AutomaticValidationFailed || x.Upload.Status == ExtractUploadStatus.ManualValidationFailed
                    ? 0
                    : x.Upload.Status == ExtractUploadStatus.Processing
                        ? 1
                        : 2)
            .ThenByDescending(x => x.Download.RequestedOn);

        query = query.Skip(request.PageIndex * request.PageSize).Take(request.PageSize + 1);

        var records = await query.ToListAsync(cancellationToken);

        return new ExtractListResponse
        {
            Items = records
                .Take(request.PageSize)
                .Select(record => new ExtractListItem
                {
                    DownloadId = new DownloadId(record.Download.DownloadId),
                    Description = record.Extract.Description,
                    ExtractRequestId = ExtractRequestId.FromString(record.Extract.ExtractRequestId),
                    RequestedOn = record.Download.RequestedOn,
                    IsInformative = record.Download.IsInformative,
                    DownloadStatus = record.Download.Status.ToString(),
                    UploadStatus = record.Upload?.Status.ToString(),
                    Closed = record.Download.Closed,
                })
                .ToList(),
            MoreDataAvailable = records.Count > request.PageSize
        };
    }
}
