namespace RoadRegistry.BackOffice.Handlers.Tests.Extracts.WhenGettingPreSignedUrlForDownloadingUploadedExtract;

using Abstractions.Uploads;
using Amazon.S3;
using Amazon.S3.Model;
using Api.Uploads;
using AutoFixture;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Configuration;
using Exceptions;
using FluentAssertions;
using FluentValidation;
using Handlers.Uploads;
using Messages;
using Moq;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Xunit.Abstractions;

public class GetUploadFilePreSignedUrlRequestTests: RoadNetworkTestBase
{
    public GetUploadFilePreSignedUrlRequestTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task WhenBlobExists_ThenPreSignedUrl()
    {
        // Arrange
        var archiveId = ObjectProvider.Create<ArchiveId>();
        var request = new GetUploadFilePreSignedUrlRequest(archiveId);

        var preSignedUrl = ObjectProvider.Create<string>();
        var amazonS3Mock = new Mock<IAmazonS3>();
        amazonS3Mock
            .Setup(x => x.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync(preSignedUrl);

        var blobClientMock = new Mock<IBlobClient>();
        blobClientMock
            .Setup(x => x.BlobExistsAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        blobClientMock
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlobObject(new BlobName(archiveId.ToString()), Metadata.None, ContentType.Parse("application/zip"), _ => null));

        // Act
        var handler = new GetUploadFilePreSignedUrlRequestHandler(
            null,
            new RoadNetworkExtractUploadsBlobClient(blobClientMock.Object),
            Clock,
            amazonS3Mock.Object,
            new S3BlobClientOptions
            {
                Buckets = new Dictionary<string, string>
                {
                    {WellKnownBuckets.UploadsBucket, WellKnownBuckets.UploadsBucket}
                }
            },
            new S3Options(),
            LoggerFactory
        );
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.PreSignedUrl.Should().Be(preSignedUrl);
    }

    [Fact]
    public async Task WhenIdentifierIsMissing_ThenValidationException()
    {
        // Arrange
        var request = new GetUploadFilePreSignedUrlRequest(null);

        // Act
        var handler = new GetUploadFilePreSignedUrlRequestHandler(
            (_, _) => Task.CompletedTask,
            new RoadNetworkExtractUploadsBlobClient(Mock.Of<IBlobClient>()),
            Clock,
            null,
            null,
            null,
            LoggerFactory
        );
        var act= () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task WhenBlobDoesNotExist_ThenExtractDownloadNotFoundException()
    {
        // Arrange
        var archiveId = ObjectProvider.Create<ArchiveId>();
        var request = new GetUploadFilePreSignedUrlRequest(archiveId);

        // Act
        var handler = new GetUploadFilePreSignedUrlRequestHandler(
            (_, _) => Task.CompletedTask,
            new RoadNetworkExtractUploadsBlobClient(Mock.Of<IBlobClient>()),
            Clock,
            null,
            null,
            null,
            LoggerFactory
        );
        var act= () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ExtractDownloadNotFoundException>();
    }
}
