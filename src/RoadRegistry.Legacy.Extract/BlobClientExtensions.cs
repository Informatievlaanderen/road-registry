namespace RoadRegistry.Legacy.Extract;

using Amazon.S3;
using BackOffice;
using BackOffice.Configuration;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
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
                var s3Options = host.Services.GetRequiredService<S3Options>();

                if(s3Options?.ServiceUrl is not null) {
                    var s3Client = host.Services.GetRequiredService<AmazonS3Client>();
                    var s3BlobClientOptions = host.Services.GetRequiredService<S3BlobClientOptions>();
                    var buckets = await s3Client.ListBucketsAsync(token);
                    if (!buckets.Buckets.Exists(bucket => bucket.BucketName == s3BlobClientOptions.Buckets[WellKnownBuckets.ImportLegacyBucket]))
                    {
                        await s3Client.PutBucketAsync(s3BlobClientOptions.Buckets[WellKnownBuckets.ImportLegacyBucket], token);
                    }
                }

                break;
            case FileBlobClient _:
                var fileOptions = host.Services.GetRequiredService<FileBlobClientOptions>();

                Directory.CreateDirectory(fileOptions.Directory);
                break;
        }
    }
}
