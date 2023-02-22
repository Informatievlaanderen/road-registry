namespace RoadRegistry.BackOffice;

using System;
using Amazon.S3;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Configuration;

public interface IBlobClientFactory
{
    IBlobClient Create(string bucketKey);
}

public class BlobClientFactory: IBlobClientFactory
{
    private readonly Lazy<AmazonS3Client> _amazonS3Client;
    private readonly BlobClientOptions _blobClientOptions;
    private readonly Lazy<FileBlobClient> _fileBlobClient;
    private readonly Lazy<S3BlobClientOptions> _s3BlobClientOptions;

    public BlobClientFactory(BlobClientOptions blobClientOptions, Lazy<AmazonS3Client> amazonS3Client, Lazy<S3BlobClientOptions> s3BlobClientOptions, Lazy<FileBlobClient> fileBlobClient)
    {
        _blobClientOptions = blobClientOptions;
        _amazonS3Client = amazonS3Client;
        _s3BlobClientOptions = s3BlobClientOptions;
        _fileBlobClient = fileBlobClient;
    }

    public IBlobClient Create(string bucketKey)
    {
        switch (_blobClientOptions.BlobClientType)
        {
            case nameof(S3BlobClient):
                return new S3BlobClient(_amazonS3Client.Value, _s3BlobClientOptions.Value.Buckets[bucketKey]);
            case nameof(FileBlobClient):
                return _fileBlobClient.Value;
        }

        throw new InvalidOperationException(_blobClientOptions.BlobClientType + " is not a supported blob client type.");
    }
}