namespace RoadRegistry.BackOffice;

using System;
using Amazon.S3;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Be.Vlaanderen.Basisregisters.BlobStore.Memory;
using Configuration;

public interface IBlobClientFactory
{
    IBlobClient Create(string bucketKey);
}

public class BlobClientFactory: IBlobClientFactory
{
    private readonly AmazonS3Client _amazonS3Client;
    private readonly BlobClientOptions _blobClientOptions;
    private readonly FileBlobClient _fileBlobClient;
    private readonly S3BlobClientOptions _s3BlobClientOptions;

    public BlobClientFactory(BlobClientOptions blobClientOptions, AmazonS3Client amazonS3Client, S3BlobClientOptions s3BlobClientOptions, FileBlobClient fileBlobClient)
    {
        _blobClientOptions = blobClientOptions;
        _amazonS3Client = amazonS3Client;
        _s3BlobClientOptions = s3BlobClientOptions;
        _fileBlobClient = fileBlobClient;
    }

    public IBlobClient Create(string bucketKey)
    {
        if (_blobClientOptions.BlobClientType == null)
        {
            return new MemoryBlobClient();
        }

        switch (_blobClientOptions.BlobClientType)
        {
            case nameof(S3BlobClient):
                return new S3BlobClient(_amazonS3Client, _s3BlobClientOptions.Buckets[bucketKey]);
            case nameof(FileBlobClient):
                return _fileBlobClient;
        }

        throw new InvalidOperationException(_blobClientOptions.BlobClientType + " is not a supported blob client type.");
    }
}
