namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using BackOffice.Configuration;
using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Microsoft.Extensions.DependencyInjection;

public static class AmazonS3Extensions
{
    public static async Task CreateMissingBucketsAsync(this IAmazonS3 amazonS3Client, string[] bucketNames, CancellationToken cancellationToken)
    {
        var buckets = await amazonS3Client.ListBucketsAsync(cancellationToken);
        var existingBucketNames = buckets.Buckets.Select(x => x.BucketName).ToArray();
        var missingBucketNames = bucketNames
            .Where(x => !existingBucketNames.Contains(x))
            .ToArray();

        foreach (var bucketName in missingBucketNames)
        {
            try
            {
                await amazonS3Client.PutBucketAsync(bucketName, cancellationToken);
            }
            catch (AmazonS3Exception)
            {
                // ignore if bucket already was created by a different host
            }
        }
    }

    public static async Task CreateMissingBucketsAsync(this IServiceProvider sp, CancellationToken cancellationToken)
    {
        var bucketNames = GetRequiredBucketNames(sp).ToArray();
        if (bucketNames.Any())
        {
            var amazonS3Client = sp.GetRequiredService<IAmazonS3>();
            await amazonS3Client.CreateMissingBucketsAsync(bucketNames, cancellationToken);
        }
    }

    private static IEnumerable<string> GetRequiredBucketNames(IServiceProvider sp)
    {
        var blobClientOptions = sp.GetService<BlobClientOptions>();
        if (blobClientOptions?.BlobClientType == nameof(S3BlobClient))
        {
            var s3BlobClientOptions = sp.GetRequiredService<S3BlobClientOptions>();
            if (s3BlobClientOptions.Buckets != null)
            {
                foreach (var bucket in s3BlobClientOptions.Buckets)
                {
                    yield return bucket.Value;
                }
            }
        }

        var distributedS3CacheOptions = sp.GetService<DistributedS3CacheOptions>();
        if (!string.IsNullOrEmpty(distributedS3CacheOptions?.Bucket))
        {
            yield return distributedS3CacheOptions.Bucket;
        }
    }
}
