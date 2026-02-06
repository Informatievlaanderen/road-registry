namespace RoadRegistry.BackOffice.Api.Handlers.Extracts;

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
using RoadRegistry.Extensions;
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
            join extractUpload in _extractsDbContext.ExtractUploads
                on extractDownload.LatestUploadId equals extractUpload.UploadId
                into extractUploads
            from extractUpload in extractUploads.DefaultIfEmpty()
            where extractDownload.DownloadId == request.DownloadId.ToGuid()
            select new { Download = extractDownload, extractRequest.Description, extractRequest.ExternalRequestId, Upload = extractUpload }
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
            ExternalExtractRequestId = record.ExternalRequestId is not null
                ? new ExternalExtractRequestId(record.ExternalRequestId)
                : null,
            RequestedOn = record.Download.RequestedOn,
            IsInformative = record.Download.IsInformative,
            UploadId = UploadId.FromValue(record.Upload?.UploadId),
            TicketId = TicketId.FromValue(record.Upload?.TicketId ?? record.Download.TicketId),
            DownloadStatus = record.Download.Status.ToString(),
            DownloadedOn = record.Download.DownloadedOn,
            UploadStatus = record.Upload?.Status.ToString(),
            Closed = record.Download.Closed
        };
    }
}
