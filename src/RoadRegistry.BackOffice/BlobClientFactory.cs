namespace RoadRegistry.BackOffice;

using Amazon.S3;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Configuration;
using System;
using Microsoft.Extensions.Logging;
using S3BlobClient = Infrastructure.S3BlobClient;

public interface IBlobClientFactory
{
    IBlobClient Create(string bucketKey, bool malwareScan = false);
}

public class BlobClientFactory: IBlobClientFactory
{
    private readonly Lazy<IAmazonS3> _amazonS3Client;
    private readonly BlobClientOptions _blobClientOptions;
    private readonly Lazy<FileBlobClient> _fileBlobClient;
    private readonly Lazy<S3BlobClientOptions> _s3BlobClientOptions;
    private readonly ILogger _logger;

    public BlobClientFactory(BlobClientOptions blobClientOptions, Lazy<IAmazonS3> amazonS3Client, Lazy<S3BlobClientOptions> s3BlobClientOptions, Lazy<FileBlobClient> fileBlobClient, ILoggerFactory loggerFactory)
    {
        _blobClientOptions = blobClientOptions;
        _amazonS3Client = amazonS3Client;
        _s3BlobClientOptions = s3BlobClientOptions;
        _fileBlobClient = fileBlobClient;
        _logger = loggerFactory.CreateLogger<BlobClientFactory>();
    }

    public IBlobClient Create(string bucketKey, bool malwareScan = false)
    {
        switch (_blobClientOptions.BlobClientType)
        {
            case nameof(S3BlobClient):
                return new S3BlobClient(_amazonS3Client.Value, _s3BlobClientOptions.Value.GetBucketName(bucketKey), malwareScan, _logger);
            case nameof(FileBlobClient):
                return _fileBlobClient.Value;
        }

        throw new InvalidOperationException(_blobClientOptions.BlobClientType + " is not a supported blob client type.");
    }
}
