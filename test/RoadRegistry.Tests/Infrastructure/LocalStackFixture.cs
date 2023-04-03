namespace RoadRegistry.Tests.Infrastructure;

using Amazon.Runtime;
using Amazon.S3;

public class LocalStackFixture : IAsyncLifetime
{
    public AmazonS3Client AmazonS3Client { get; private set; }
    public AmazonS3Config AmazonS3Config { get; private set; }

    public async Task InitializeAsync()
    {
        var credentials = new BasicAWSCredentials("dummy", "dummy");

        AmazonS3Config = new()
        {
            ServiceURL = "http://localhost:4566",
            ForcePathStyle = true
        };
        AmazonS3Client = new(credentials, AmazonS3Config);

        await AmazonS3Client.PutBucketAsync("road-registry-tests", CancellationToken.None);
    }

    public Task DisposeAsync()
    {
        AmazonS3Client.Dispose();

        return Task.CompletedTask;
    }
}