namespace RoadRegistry.Legacy.Extract;

using Amazon.S3;
using BackOffice;
using BackOffice.Configuration;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

internal static class BlobClientExtensions
{
    public static async Task ProvisionResources(this IBlobClient client, IHost host, CancellationToken token = default)
    {
        switch (client)
        {
            case S3BlobClient _:
                if (Environment.GetEnvironmentVariable("MINIO_SERVER") != null)
                {
                    var s3Client = host.Services.GetRequiredService<AmazonS3Client>();
                    var s3Options = host.Services.GetRequiredService<S3BlobClientOptions>();

                    var buckets = await s3Client.ListBucketsAsync(token);
                    if (!buckets.Buckets.Exists(bucket => bucket.BucketName == s3Options.Buckets[WellknownBuckets.ImportLegacyBucket])) await s3Client.PutBucketAsync(s3Options.Buckets[WellknownBuckets.ImportLegacyBucket], token);
                }

                break;
            case FileBlobClient _:
                var fileOptions = host.Services.GetRequiredService<FileBlobClientOptions>();

                Directory.CreateDirectory(fileOptions.Directory);
                break;
        }
    }
}
