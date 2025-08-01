namespace RoadRegistry.BackOffice.Api.Handlers.Extracts;

using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions;
using Abstractions.Exceptions;
using Abstractions.Extracts.V2;
using Amazon.S3;
using Amazon.S3.Model;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Configuration;
using Exceptions;
using Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.Extracts.Schema;
using SqlStreamStore;

public class GetDownloadExtractPreSignedUrlRequestHandler : EndpointRequestHandler<GetDownloadExtractPreSignedUrlRequest, GetDownloadExtractPreSignedUrlResponse>
{
    private readonly ExtractsDbContext _extractsDbContext;
    private readonly RoadNetworkExtractDownloadsBlobClient _client;
    private readonly IClock _clock;
    private readonly IAmazonS3 _amazonS3;
    private readonly S3BlobClientOptions _s3BlobClientOptions;
    private readonly S3Options _s3Options;

    public GetDownloadExtractPreSignedUrlRequestHandler(
        CommandHandlerDispatcher dispatcher,
        ExtractsDbContext extractsDbContext,
        RoadNetworkExtractDownloadsBlobClient client,
        IStreamStore streamStore,
        IClock clock,
        IAmazonS3 amazonS3,
        S3BlobClientOptions s3BlobClientOptions,
        S3Options s3Options,
        ILoggerFactory loggerFactory)
        : base(dispatcher, loggerFactory.CreateLogger<GetDownloadExtractPreSignedUrlRequestHandler>())
    {
        _extractsDbContext = extractsDbContext;
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _clock = clock;
        _amazonS3 = amazonS3;
        _s3BlobClientOptions = s3BlobClientOptions;
        _s3Options = s3Options;
    }

    protected override async Task<GetDownloadExtractPreSignedUrlResponse> InnerHandleAsync(GetDownloadExtractPreSignedUrlRequest request, CancellationToken cancellationToken)
    {
        var record = await _extractsDbContext.ExtractRequests.SingleOrDefaultAsync(x => x.DownloadId == request.DownloadId.ToGuid(), cancellationToken);

        if (record is null || record is not { DownloadAvailable: true })
        {
            throw new ExtractDownloadNotFoundException(request.DownloadId);
        }

        if (string.IsNullOrEmpty(record.ArchiveId))
        {
            throw new ExtractArchiveNotCreatedException();
        }

        var blobName = new BlobName(record.ArchiveId);

        if (!await _client.BlobExistsAsync(blobName, cancellationToken))
        {
            throw new BlobNotFoundException(blobName);
        }

        var bucketName = _s3BlobClientOptions.GetBucketName(WellKnownBuckets.ExtractDownloadsBucket);

        var preSignedUrl = await _amazonS3.GetPreSignedURLAsync(
            new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = blobName,
                Expires = _clock
                    .GetCurrentInstant()
                    .Plus(Duration.FromSeconds(_s3Options.ExpiresInSeconds))
                    .ToDateTimeUtc()
            });

        record.DownloadedOn = DateTimeOffset.UtcNow;
        await _extractsDbContext.SaveChangesAsync(cancellationToken);

        return new GetDownloadExtractPreSignedUrlResponse(preSignedUrl);
    }
}
