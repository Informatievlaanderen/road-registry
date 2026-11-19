namespace RoadRegistry.BackOffice.Api.Tests.Infrastructure;

using System.Collections.Generic;
using Amazon.S3;
using Amazon.S3.Model;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentAssertions;
using Moq;
using NodaTime;
using NodaTime.Testing;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Configuration;

public class AmazonS3DownloadFileUrlPresignerTests
{
    // The presigned url points to S3, another origin than the portal, so the browser ignores the download attribute on
    // the link. The file name only reaches the browser as the Content-Disposition of the response S3 sends.
    [Fact]
    public async Task ThenTheUrlAsksS3ToSendTheFileNameAsContentDisposition()
    {
        GetPreSignedUrlRequest? sentRequest = null;
        var amazonS3 = new Mock<IAmazonS3>();
        amazonS3
            .Setup(x => x.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .Callback<GetPreSignedUrlRequest>(request => sentRequest = request)
            .ReturnsAsync("https://bucket.s3.amazonaws.com/blob");

        var presigner = new AmazonS3DownloadFileUrlPresigner(
            new FakeClock(Instant.FromUtc(2026, 11, 18, 9, 0)),
            amazonS3.Object,
            new S3Options(),
            new S3BlobClientOptions { Buckets = new Dictionary<string, string> { { "uploads", "uploads-bucket" } } });

        var response = await presigner.CreatePresignedDownloadUrl("uploads", new BlobName("blob"), "download_upload_202611131405.zip");

        response.FileName.Should().Be("download_upload_202611131405.zip");
        sentRequest.Should().NotBeNull();
        sentRequest!.BucketName.Should().Be("uploads-bucket");
        sentRequest.ResponseHeaderOverrides.ContentDisposition.Should().Be("attachment; filename=\"download_upload_202611131405.zip\"");
    }
}
