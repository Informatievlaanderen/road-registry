namespace RoadRegistry.BackOffice.Api.Extracts.Handlers;

using System;
using System.Threading;
using System.Threading.Tasks;
using Abstractions.Exceptions;
using Abstractions.Extracts;
using Amazon.S3;
using Amazon.S3.Model;
using BackOffice.Extracts;
using BackOffice.Handlers;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Configuration;
using Editor.Schema;
using Framework;
using Messages;
using Microsoft.Extensions.Logging;
using NodaTime;
using SqlStreamStore;

public class GetDownloadFilePreSignedUrlRequestHandler : EndpointRetryableRequestHandler<GetDownloadFilePreSignedUrlRequest, GetDownloadFilePreSignedUrlResponse>
{
    private readonly RoadNetworkExtractDownloadsBlobClient _client;
    private readonly IAmazonS3 _amazonS3;
    private readonly S3BlobClientOptions _s3BlobClientOptions;
    private readonly S3Options _s3Options;

    public GetDownloadFilePreSignedUrlRequestHandler(
        CommandHandlerDispatcher dispatcher,
        EditorContext editorContext,
        RoadNetworkExtractDownloadsBlobClient client,
        IStreamStore streamStore,
        IClock clock,
        IAmazonS3 amazonS3,
        S3BlobClientOptions s3BlobClientOptions,
        S3Options s3Options,
        ILoggerFactory loggerFactory)
        : base(dispatcher, editorContext, streamStore, clock, loggerFactory.CreateLogger<GetDownloadFilePreSignedUrlRequestHandler>())
    {
        _client = client ?? throw new ArgumentNullException(nameof(client));
        _amazonS3 = amazonS3;
        _s3BlobClientOptions = s3BlobClientOptions;
        _s3Options = s3Options;
    }

    protected override async Task<GetDownloadFilePreSignedUrlResponse> InnerHandleAsync(GetDownloadFilePreSignedUrlRequest request, CancellationToken cancellationToken)
    {
        if (request.DownloadId is null)
        {
            throw new DownloadExtractNotFoundException("Could not find extract with empty download identifier");
        }

        if (!DownloadId.TryParse(request.DownloadId, out var downloadId))
        {
            throw new InvalidGuidValidationException(nameof(request.DownloadId));
        }

        var record = await Context.ExtractDownloads.FindAsync(new object[] { downloadId.ToGuid() }, cancellationToken);

        if (record is null || record is not { Available: true })
        {
            var retryAfter = await CalculateRetryAfterAsync(request, cancellationToken);
            throw new DownloadExtractNotFoundException(Convert.ToInt32(retryAfter.TotalSeconds));
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

        var command = new Command(new DownloadRoadNetworkExtract
        {
            DownloadId = downloadId,
            ExternalRequestId = new ExternalExtractRequestId(record.ExternalRequestId)
        }).WithProvenanceData(request.ProvenanceData);
        await Dispatch(command, cancellationToken);

        var bucketName = _s3BlobClientOptions.GetBucketName(WellKnownBuckets.ExtractDownloadsBucket);

        var preSignedUrl = await _amazonS3.GetPreSignedURLAsync(
            new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = blobName,
                Expires = Clock
                    .GetCurrentInstant()
                    .Plus(Duration.FromSeconds(_s3Options.ExpiresInSeconds))
                    .ToDateTimeUtc()
            });

        return new GetDownloadFilePreSignedUrlResponse(preSignedUrl);
    }
}
