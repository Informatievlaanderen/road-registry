namespace RoadRegistry.BackOffice;

using Amazon.S3;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Configuration;
using System;
using S3BlobClient = Infrastructure.S3BlobClient;

public interface IBlobClientFactory
{
    IBlobClient Create(string bucketKey);
}

public class BlobClientFactory: IBlobClientFactory
{
    private readonly Lazy<IAmazonS3> _amazonS3Client;
    private readonly BlobClientOptions _blobClientOptions;
    private readonly Lazy<FileBlobClient> _fileBlobClient;
    private readonly Lazy<S3BlobClientOptions> _s3BlobClientOptions;

    public BlobClientFactory(BlobClientOptions blobClientOptions, Lazy<IAmazonS3> amazonS3Client, Lazy<S3BlobClientOptions> s3BlobClientOptions, Lazy<FileBlobClient> fileBlobClient)
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
                return new S3BlobClient(_amazonS3Client.Value, _s3BlobClientOptions.Value.GetBucketName(bucketKey));
            case nameof(FileBlobClient):
                return _fileBlobClient.Value;
        }

        throw new InvalidOperationException(_blobClientOptions.BlobClientType + " is not a supported blob client type.");
    }
}
