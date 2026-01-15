namespace RoadRegistry.BackOffice.Api.Handlers.Inwinning;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.Extracts.Schema;

public class InwinningExtractListRequestHandler : EndpointRequestHandler<InwinningExtractListRequest, ExtractListResponse>
{
    private readonly ExtractsDbContext _extractsDbContext;

    public InwinningExtractListRequestHandler(
        ExtractsDbContext extractsDbContext,
        CommandHandlerDispatcher dispatcher,
        ILogger<InwinningExtractListRequestHandler> logger)
        : base(dispatcher, logger)
    {
        _extractsDbContext = extractsDbContext;
    }

    protected override async Task<ExtractListResponse> InnerHandleAsync(InwinningExtractListRequest request, CancellationToken cancellationToken)
    {
        var extractRequestsQuery = _extractsDbContext.ExtractRequests
            .Where(er => er.OrganizationCode == request.OrganizationCode && er.ExternalRequestId.StartsWith("INWINNING_"));

        var query =
                from extractRequest in extractRequestsQuery
                join extractDownload in _extractsDbContext.ExtractDownloads on extractRequest.CurrentDownloadId equals extractDownload.DownloadId
                select new { Extract = extractRequest, Download = extractDownload }
            ;

        query = query
            .OrderBy(x => x.Download.Closed
                ? 9
                : x.Download.UploadStatus == ExtractUploadStatus.Rejected
                    ? 0
                    : x.Download.UploadStatus == ExtractUploadStatus.Processing
                        ? 1
                        : 2)
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
                    RequestedOn = record.Download.RequestedOn,
                    IsInformative = record.Download.IsInformative,
                    DownloadStatus = record.Download.DownloadStatus.ToString(),
                    UploadStatus = record.Download.UploadStatus?.ToString(),
                    Closed = record.Download.Closed,
                })
                .ToList()
        };
    }
}
