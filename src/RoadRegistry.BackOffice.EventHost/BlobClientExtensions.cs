namespace RoadRegistry.BackOffice.EventHost;

using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.BlobStore.Aws;
using Be.Vlaanderen.Basisregisters.BlobStore.IO;
using Hosts;
using Hosts.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

internal static class BlobClientExtensions
{
    public static async Task ProvisionResources(this IBlobClient client, IHost host, CancellationToken token = default)
    {
        var configuration = host.Services.GetRequiredService<IConfiguration>();

        switch (client)
        {
            case S3BlobClient _:
                if (Environment.GetEnvironmentVariable("MINIO_SERVER") != null)
                {
                    var s3Client = host.Services.GetRequiredService<AmazonS3Client>();
                    var s3Options = new S3BlobClientOptions();
                    configuration.GetSection(nameof(S3BlobClientOptions)).Bind(s3Options);

                    var buckets = await s3Client.ListBucketsAsync(token);
                    var bucketNames = typeof(WellknownBuckets)
                        .GetFields()
                        .Select(fi => (string)fi.GetValue(null))
                        .ToArray();

                    foreach (var bucketName in bucketNames)
                    {
                        if (s3Options.Buckets.ContainsKey(bucketName) && !buckets.Buckets.Exists(bucket => bucket.BucketName == s3Options.Buckets[bucketName]))
                        {
                            await s3Client.PutBucketAsync(s3Options.Buckets[bucketName], token);
                        }
                    }
                }
                break;
            case FileBlobClient _:
                var fileOptions = new FileBlobClientOptions();
                configuration.GetSection(nameof(FileBlobClientOptions)).Bind(fileOptions);

                Directory.CreateDirectory(fileOptions.Directory);
                break;
        }
    }
}
