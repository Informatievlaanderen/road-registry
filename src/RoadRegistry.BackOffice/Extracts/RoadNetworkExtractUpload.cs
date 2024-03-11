namespace RoadRegistry.BackOffice.Extracts;

using Amazon.SimpleNotificationService.Model;
using Messages;
using Newtonsoft.Json;
using System;
using System.IO.Compression;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TicketingService.Abstractions;
using Uploads;

public class RoadNetworkExtractUpload
{
    private readonly Action<object> _applier;
    private readonly ArchiveId _archiveId;
    private readonly DownloadId _downloadId;
    private readonly ExternalExtractRequestId _externalRequestId;
    private readonly ExtractRequestId _requestId;
    private readonly ExtractDescription _extractDescription;
    private readonly UploadId _uploadId;

    internal RoadNetworkExtractUpload(ExternalExtractRequestId externalRequestId, ExtractRequestId requestId, ExtractDescription extractDescription, DownloadId downloadId, UploadId uploadId, ArchiveId archiveId, Action<object> applier)
    {
        _externalRequestId = externalRequestId;
        _requestId = requestId;
        _extractDescription = extractDescription;
        _downloadId = downloadId;
        _uploadId = uploadId;
        _archiveId = archiveId;
        _applier = applier;
    }

    private void Apply(object @event)
    {
        _applier(@event);
    }

    public async Task<ZipArchiveProblems> ValidateArchiveUsing(ZipArchive archive, Guid? ticketId,
        IZipArchiveValidator validator,
        IExtractUploadFailedEmailClient emailClient,
        ITicketing ticketing,
        CancellationToken cancellationToken)
    {
        var zipArchiveMetadata = ZipArchiveMetadata.Empty.WithDownloadId(_downloadId);

        var problems = await validator.ValidateAsync(archive, new ZipArchiveValidatorContext(zipArchiveMetadata), cancellationToken);
        if (!problems.HasError())
        {
            Apply(
                new RoadNetworkExtractChangesArchiveAccepted
                {
                    Description = _extractDescription,
                    ExternalRequestId = _externalRequestId,
                    RequestId = _requestId,
                    DownloadId = _downloadId,
                    UploadId = _uploadId,
                    ArchiveId = _archiveId,
                    Problems = problems.Select(problem => problem.Translate()).ToArray(),
                    TicketId = ticketId
                });
        }
        else
        {
            var @event =
                new RoadNetworkExtractChangesArchiveRejected
                {
                    Description = _extractDescription,
                    ExternalRequestId = _externalRequestId,
                    RequestId = _requestId,
                    DownloadId = _downloadId,
                    UploadId = _uploadId,
                    ArchiveId = _archiveId,
                    Problems = problems.Select(problem => problem.Translate()).ToArray(),
                    TicketId = ticketId
                };

            Apply(@event);

            if (ticketId is not null)
            {
                var errors = problems.Select(x => x.Translate().ToTicketError()).ToArray();
                await ticketing.Error(ticketId.Value, new TicketError(errors), cancellationToken);
            }

            if (emailClient is not null)
            {
                await emailClient.SendAsync(_extractDescription, new ValidationException(JsonConvert.SerializeObject(@event, Formatting.Indented)), cancellationToken);
            }
        }

        return problems;
    }
}
