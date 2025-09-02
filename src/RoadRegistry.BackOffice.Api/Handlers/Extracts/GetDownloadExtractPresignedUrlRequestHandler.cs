namespace RoadRegistry.BackOffice.Api.Handlers.Extracts;

using System;
using System.Threading;
using System.Threading.Tasks;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.Extracts.Schema;

public class GetDownloadExtractPresignedUrlRequestHandler : EndpointRequestHandler<GetDownloadExtractPresignedUrlRequest, GetDownloadExtractPresignedUrlResponse>
{
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly RoadNetworkExtractDownloadsBlobClient _client;
    private readonly IDownloadFileUrlPresigner _downloadFileUrlPresigner;

    public GetDownloadExtractPresignedUrlRequestHandler(
        CommandHandlerDispatcher dispatcher,
        ExtractsDbContext extractsDbContext,
        RoadNetworkExtractDownloadsBlobClient client,
        IDownloadFileUrlPresigner downloadFileUrlPresigner,
        ILoggerFactory loggerFactory)
        : base(dispatcher, loggerFactory.CreateLogger<GetDownloadExtractPresignedUrlRequestHandler>())
    {
        _extractsDbContext = extractsDbContext;
        _client = client;
        _downloadFileUrlPresigner = downloadFileUrlPresigner;
    }

    protected override async Task<GetDownloadExtractPresignedUrlResponse> InnerHandleAsync(GetDownloadExtractPresignedUrlRequest request, CancellationToken cancellationToken)
    {
        var record = await _extractsDbContext.ExtractDownloads.SingleOrDefaultAsync(x => x.DownloadId == request.DownloadId.ToGuid(), cancellationToken);

        if (record is null || record is not { DownloadStatus: ExtractDownloadStatus.Available })
        {
            throw new ExtractDownloadNotFoundException(request.DownloadId);
        }

        var blobName = new BlobName(request.DownloadId);

        if (!await _client.BlobExistsAsync(blobName, cancellationToken))
        {
            throw new BlobNotFoundException(blobName);
        }

        var fileName = $"{blobName}.zip";
        var presignedUrl = await _downloadFileUrlPresigner.CreatePresignedDownloadUrl(WellKnownBuckets.ExtractDownloadsBucket, blobName, fileName);

        record.DownloadedOn = DateTimeOffset.UtcNow;
        await _extractsDbContext.SaveChangesAsync(cancellationToken);

        return new GetDownloadExtractPresignedUrlResponse(presignedUrl.Url.ToString());
    }
}
