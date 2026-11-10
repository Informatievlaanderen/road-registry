namespace RoadRegistry.BackOffice.Api.Handlers.Extracts;

using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.Extensions;
using RoadRegistry.Extracts.Schema;

public class GetDownloadUploadPresignedUrlRequestHandler : EndpointRequestHandler<GetDownloadUploadPresignedUrlRequest, GetDownloadUploadPresignedUrlResponse>
{
    private readonly RoadNetworkExtractUploadsBlobClient _client;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IDownloadFileUrlPresigner _downloadFileUrlPresigner;

    public GetDownloadUploadPresignedUrlRequestHandler(
        CommandHandlerDispatcher dispatcher,
        ExtractsDbContext extractsDbContext,
        RoadNetworkExtractUploadsBlobClient client,
        IDownloadFileUrlPresigner downloadFileUrlPresigner,
        ILoggerFactory logger)
        : base(dispatcher, logger.CreateLogger<GetDownloadUploadPresignedUrlRequestHandler>())
    {
        _client = client;
        _extractsDbContext = extractsDbContext;
        _downloadFileUrlPresigner = downloadFileUrlPresigner;
    }

    protected override async Task<GetDownloadUploadPresignedUrlResponse> InnerHandleAsync(GetDownloadUploadPresignedUrlRequest request, CancellationToken cancellationToken)
    {
        var downloadRecord = await _extractsDbContext.ExtractDownloads.SingleOrDefaultAsync(x => x.DownloadId == request.DownloadId.ToGuid(), cancellationToken);

        if (downloadRecord is not { Status: ExtractDownloadStatus.Available })
        {
            throw new ExtractDownloadNotFoundException(request.DownloadId);
        }

        if (downloadRecord.LatestUploadId is null)
        {
            throw new ExtractUploadNotFoundException(request.DownloadId);
        }

        var uploadId = new UploadId(downloadRecord.LatestUploadId.Value);
        var blobName = new BlobName(uploadId);

        if (!await _client.BlobExistsAsync(blobName, cancellationToken))
        {
            throw new BlobNotFoundException(blobName);
        }

        // The file name the uploader gave is not kept reliably: often it is just a guid, without the .zip extension.
        // The name is therefore built from the download and the moment of the upload, in the time zone of the portal.
        var upload = await _extractsDbContext.ExtractUploads
            .SingleOrDefaultAsync(x => x.UploadId == uploadId.ToGuid(), cancellationToken);
        var downloadIdWithoutDashes = request.DownloadId.ToGuid().ToString("N");
        var fileName = upload is not null
            ? $"{downloadIdWithoutDashes}_upload_{Instant.FromDateTimeOffset(upload.UploadedOn).ToBrusselsDateTime().ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture)}.zip"
            : $"{downloadIdWithoutDashes}_upload.zip";

        var presignedUrl = await _downloadFileUrlPresigner.CreatePresignedDownloadUrl(WellKnownBuckets.UploadsBucket, blobName, fileName);

        return new GetDownloadUploadPresignedUrlResponse(presignedUrl.Url.ToString(), presignedUrl.FileName);
    }
}
