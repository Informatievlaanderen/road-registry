namespace RoadRegistry.Tests.Infrastructure;

using Amazon.S3.Model;
using Be.Vlaanderen.Basisregisters.BlobStore;
using RoadRegistry.BackOffice.Infrastructure;

public class LocalStackTests : IClassFixture<LocalStackFixture>
{
    private readonly LocalStackFixture _fixture;

    public LocalStackTests(LocalStackFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact(Skip = "Localhost only test")]
    public async Task ItShouldBeAbleToListBuckets()
    {
        var buckets = await _fixture.AmazonS3Client.ListBucketsAsync(CancellationToken.None);

        Assert.NotEmpty(buckets.Buckets);
    }

    [Fact(Skip = "Localhost only test")]
    public async Task ItShouldHaveBlobExists()
    {
        var filename = "s3-bucket-testeroo.zip";
    }

    [Fact(Skip = "Localhost only test")]
    public async Task ItShouldPutObjectWithAmazonS3Client()
    {
        var fileName = "s3-bucket-tester.zip";

        var sourceStream = await EmbeddedResourceReader.ReadAsync(fileName);
        var putObjectRequest = new PutObjectRequest
        {
            Key = fileName,
            BucketName = "road-registry-tests",
            InputStream = sourceStream
        };
        var putObjectResponse = await _fixture.AmazonS3Client.PutObjectAsync(putObjectRequest);

        var getObjectRequest = new GetObjectRequest
        {
            Key = fileName,
            BucketName = "road-registry-tests"
        };
        var getObjectResponse = await _fixture.AmazonS3Client.GetObjectAsync(getObjectRequest);
    }

    [Fact(Skip = "Localhost only test")]
    public async Task ItShouldPutObjectWithS3BlobClient()
    {
        var fileName = "s3-bucket-tester.zip";
        var sourceStream = await EmbeddedResourceReader.ReadAsync(fileName);

        //var client = new Be.Vlaanderen.Basisregisters.BlobStore.Aws.S3BlobClient(_fixture.AmazonS3Client, "road-registry-tests");
        var client = new S3BlobClient(_fixture.AmazonS3Client, "road-registry-tests");

        await client.CreateBlobAsync(
            new BlobName(fileName),
            Metadata.None,
            ContentType.Parse("application/zip"),
            sourceStream,
            CancellationToken.None
        );

        var blob = await client.GetBlobAsync(new BlobName(fileName), CancellationToken.None);
    }
}