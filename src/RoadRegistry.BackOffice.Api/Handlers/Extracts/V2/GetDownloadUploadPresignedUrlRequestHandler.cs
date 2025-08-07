namespace RoadRegistry.BackOffice.Api.Handlers.Extracts.V2;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Extracts.V2;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using Framework;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.Extracts.Schema;

public class GetDownloadUploadPresignedUrlRequestHandler : EndpointRequestHandler<GetDownloadUploadPresignedUrlRequest, GetDownloadUploadPresignedUrlResponse>
{
    private readonly RoadNetworkExtractUploadsBlobClient _client;
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly IDownloadFileUrlPresigner _downloadFileUrlPresigner;

    public GetDownloadUploadPresignedUrlRequestHandler(
        CommandHandlerDispatcher dispatcher,
        RoadNetworkExtractUploadsBlobClient client,
        ExtractsDbContext extractsDbContext,
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
        var record = await _extractsDbContext.ExtractDownloads.SingleOrDefaultAsync(x => x.DownloadId == request.DownloadId.ToGuid(), cancellationToken);

        if (record is null || record is not { DownloadAvailable: true })
        {
            throw new ExtractDownloadNotFoundException(request.DownloadId);
        }

        if (record.UploadId is null)
        {
            throw new ExtractUploadNotFoundException(request.DownloadId);
        }

        var uploadId = new UploadId(record.UploadId.Value);
        var blobName = new BlobName(uploadId);

        if (!await _client.BlobExistsAsync(blobName, cancellationToken))
        {
            throw new BlobNotFoundException(blobName);
        }

        var blob = await _client.GetBlobAsync(blobName, cancellationToken);

        var fileName = blob.Metadata
            .Where(pair => pair.Key == new MetadataKey("filename"))
            .Select(x => x.Value)
            .Single();

        var preSignedUrl = await _downloadFileUrlPresigner.CreatePresignedDownloadUrl(WellKnownBuckets.UploadsBucket, blobName, fileName);

        return new GetDownloadUploadPresignedUrlResponse(preSignedUrl.Url.ToString(), preSignedUrl.FileName);
    }
}
