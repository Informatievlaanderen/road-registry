namespace RoadRegistry.Hosts.Infrastructure.Extensions;

using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;

public static class AmazonS3Extensions
{
    public static async Task CreateMissingBucketsAsync(this AmazonS3Client amazonS3Client, string[] bucketNames, CancellationToken cancellationToken)
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
}
