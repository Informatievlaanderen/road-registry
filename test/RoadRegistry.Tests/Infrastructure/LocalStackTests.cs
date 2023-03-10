namespace RoadRegistry.Tests.Infrastructure
{
    using Amazon.S3.Model;
    using Be.Vlaanderen.Basisregisters.BlobStore;
    using System.Threading;

    public class LocalStackTests : IClassFixture<LocalStackFixture>
    {
        private readonly LocalStackFixture _fixture;
        private static readonly ByteRange NoData = new(0L, 0L);

        public LocalStackTests(LocalStackFixture fixture) => _fixture = fixture;

        [Fact]
        public async Task ItShouldBeAbleToListBuckets()
        {
            var buckets = await _fixture.AmazonS3Client.ListBucketsAsync(CancellationToken.None);

            Assert.NotEmpty(buckets.Buckets);
        }

        [Fact]
        public async Task ItShouldPutObjectWithAmazonS3Client()
        {
            var filename = "s3-bucket-tester.zip";

            using var sourceStream = new MemoryStream();
            await using (var embeddedStream =
                         typeof(LocalStackFixture).Assembly.GetManifestResourceStream(typeof(LocalStackFixture), filename))
            {
                if (embeddedStream is not null)
                {
                    await embeddedStream.CopyToAsync(sourceStream);
                }
            }

            sourceStream.Position = 0;
            var putObjectRequest = new PutObjectRequest()
            {
                Key = filename,
                BucketName = "road-registry-tests",
                InputStream = sourceStream
            };
            var putObjectResponse = await _fixture.AmazonS3Client.PutObjectAsync(putObjectRequest);

            var getObjectRequest = new GetObjectRequest()
            {
                Key = filename,
                BucketName = "road-registry-tests"
            };
            var getObjectResponse = await _fixture.AmazonS3Client.GetObjectAsync(getObjectRequest);
        }

        [Fact]
        public async Task ItShouldHaveBlobExists()
        {
            var filename = "s3-bucket-testeroo.zip";
        }

        [Fact]
        public async Task ItShouldPutObjectWithS3BlobClient()
        {
            var filename = "s3-bucket-tester.zip";

            using var sourceStream = new MemoryStream();
            await using (var embeddedStream =
                         typeof(LocalStackFixture).Assembly.GetManifestResourceStream(typeof(LocalStackFixture), filename))
            {
                if (embeddedStream is not null)
                {
                    await embeddedStream.CopyToAsync(sourceStream);
                }
            }

            sourceStream.Position = 0;

            //var client = new Be.Vlaanderen.Basisregisters.BlobStore.Aws.S3BlobClient(_fixture.AmazonS3Client, "road-registry-tests");
            var client = new RoadRegistry.BackOffice.Infrastructure.S3BlobClient(_fixture.AmazonS3Client, "road-registry-tests");

            await client.CreateBlobAsync(
                new BlobName(filename),
                Metadata.None,
                ContentType.Parse("application/zip"),
                sourceStream,
                CancellationToken.None
            );

            var blob = await client.GetBlobAsync(new BlobName(filename), CancellationToken.None);
        }
    }
}
