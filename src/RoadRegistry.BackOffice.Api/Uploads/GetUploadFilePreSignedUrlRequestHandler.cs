namespace RoadRegistry.BackOffice.Api.Uploads;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using NodaTime;
using RoadRegistry.BackOffice.Abstractions;
using RoadRegistry.BackOffice.Abstractions.Exceptions;
using RoadRegistry.BackOffice.Abstractions.Uploads;
using RoadRegistry.BackOffice.Configuration;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Framework;

public class GetUploadFilePreSignedUrlRequestHandler : EndpointRequestHandler<GetUploadFilePreSignedUrlRequest, GetUploadFilePreSignedUrlResponse>
{
    private readonly RoadNetworkExtractUploadsBlobClient _client;
    private readonly IClock _clock;
    private readonly IAmazonS3 _amazonS3;
    private readonly S3BlobClientOptions _s3BlobClientOptions;
    private readonly S3Options _s3Options;

    public GetUploadFilePreSignedUrlRequestHandler(
        CommandHandlerDispatcher dispatcher,
        RoadNetworkExtractUploadsBlobClient client,
        IClock clock,
        IAmazonS3 amazonS3,
        S3BlobClientOptions s3BlobClientOptions,
        S3Options s3Options,
        ILoggerFactory logger)
        : base(dispatcher, logger.CreateLogger<GetUploadFilePreSignedUrlRequestHandler>())
    {
        _client = client ?? throw new BlobClientNotFoundException(nameof(client));
        _clock = clock;
        _amazonS3 = amazonS3;
        _s3BlobClientOptions = s3BlobClientOptions;
        _s3Options = s3Options;
    }

    protected override async Task<GetUploadFilePreSignedUrlResponse> InnerHandleAsync(GetUploadFilePreSignedUrlRequest request, CancellationToken cancellationToken)
    {
        if (!ArchiveId.Accepts(request.Identifier))
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure(nameof(request.Identifier),
                    $"'{nameof(request.Identifier)}' path parameter cannot be empty and must be less or equal to {ArchiveId.MaxLength} characters.")
            });
        }

        var archiveId = new ArchiveId(request.Identifier);
        var blobName = new BlobName(archiveId.ToString());

        if (!await _client.BlobExistsAsync(blobName, cancellationToken))
        {
            throw new ExtractDownloadNotFoundException(request.Identifier);
        }

        var blob = await _client.GetBlobAsync(blobName, cancellationToken);

        var metadata = blob.Metadata.Where(pair => pair.Key == new MetadataKey("filename")).ToArray();
        var fileName = metadata.Length == 1 ? metadata[0].Value : archiveId + ".zip";

        var bucketName = _s3BlobClientOptions.GetBucketName(WellKnownBuckets.UploadsBucket);

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

        return new GetUploadFilePreSignedUrlResponse(preSignedUrl, fileName);
    }
}
