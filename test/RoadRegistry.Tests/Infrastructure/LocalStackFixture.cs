namespace RoadRegistry.Tests.Infrastructure;

using Amazon.Runtime;
using Amazon.S3;

public class LocalStackFixture : IAsyncLifetime
{
    public IAmazonS3 AmazonS3Client { get; private set; }
    public AmazonS3Config AmazonS3Config { get; private set; }

    public async Task InitializeAsync()
    {
        var credentials = new BasicAWSCredentials("dummy", "dummy");

        AmazonS3Config = new AmazonS3Config
        {
            ServiceURL = "http://localhost:4566",
            ForcePathStyle = true
        };
        AmazonS3Client = new AmazonS3Client(credentials, AmazonS3Config);

        await AmazonS3Client.PutBucketAsync("road-registry-tests", CancellationToken.None);
    }

    public Task DisposeAsync()
    {
        AmazonS3Client.Dispose();

        return Task.CompletedTask;
    }
}
