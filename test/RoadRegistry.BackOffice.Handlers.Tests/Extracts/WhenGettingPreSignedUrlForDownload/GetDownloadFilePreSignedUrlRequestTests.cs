namespace RoadRegistry.BackOffice.Handlers.Tests.Extracts.WhenGettingPreSignedUrlForDownload;

using Abstractions.Exceptions;
using Abstractions.Extracts;
using Amazon.S3;
using Amazon.S3.Model;
using AutoFixture;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Configuration;
using Editor.Schema;
using Editor.Schema.Extracts;
using FluentAssertions;
using Handlers.Extracts;
using Messages;
using Microsoft.EntityFrameworkCore;
using Moq;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Xunit.Abstractions;

public class GetDownloadFilePreSignedUrlRequestTests: RoadNetworkTestBase
{
    public GetDownloadFilePreSignedUrlRequestTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task WhenBlobExists_ThenPreSignedUrl()
    {
        // Arrange
        await using var editorContext = BuildEditorContext();

        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new GetDownloadFilePreSignedUrlRequest(downloadId, 0, 0);

        editorContext.ExtractDownloads.Add(new ExtractDownloadRecord
        {
            DownloadId = downloadId.ToGuid(),
            ExternalRequestId = ObjectProvider.Create<string>(),
            RequestId = ObjectProvider.Create<string>(),
            Available = true,
            ArchiveId = ObjectProvider.Create<string>()
        });
        await editorContext.SaveChangesAsync();

        var preSignedUrl = ObjectProvider.Create<string>();
        var amazonS3Mock = new Mock<IAmazonS3>();
        amazonS3Mock
            .Setup(x => x.GetPreSignedURLAsync(It.IsAny<GetPreSignedUrlRequest>()))
            .ReturnsAsync(preSignedUrl);

        var blobClientMock = new Mock<IBlobClient>();
        blobClientMock
            .Setup(x => x.BlobExistsAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var commandHandlerDispatcher = new FakeCommandHandlerDispatcher();

        // Act
        var handler = new GetDownloadFilePreSignedUrlRequestHandler(
            commandHandlerDispatcher.Dispatcher,
            editorContext,
            new RoadNetworkExtractDownloadsBlobClient(blobClientMock.Object),
            Store,
            Clock,
            amazonS3Mock.Object,
            new S3BlobClientOptions
            {
                Buckets = new Dictionary<string, string>
                {
                    {WellKnownBuckets.ExtractDownloadsBucket, WellKnownBuckets.ExtractDownloadsBucket}
                }
            },
            new S3Options(),
            LoggerFactory
        );
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.PreSignedUrl.Should().Be(preSignedUrl);
        commandHandlerDispatcher.Invocations.Should().HaveCount(1);
        commandHandlerDispatcher.Invocations.Single().Body.Should().BeOfType<DownloadRoadNetworkExtract>();
    }

    [Fact]
    public async Task WhenDownloadIdIsMissing_ThenDownloadExtractNotFoundException()
    {
        // Arrange
        await using var editorContext = BuildEditorContext();

        var request = new GetDownloadFilePreSignedUrlRequest(null, 0, 0);

        // Act
        var handler = new GetDownloadFilePreSignedUrlRequestHandler(
            (_, _) => Task.CompletedTask,
            editorContext,
            new RoadNetworkExtractDownloadsBlobClient(Mock.Of<IBlobClient>()),
            Store,
            Clock,
            null,
            null,
            null,
            LoggerFactory
        );
        var act= () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DownloadExtractNotFoundException>();
    }

    [Fact]
    public async Task WhenDownloadIdIsNotAGuid_ThenInvalidGuidValidationException()
    {
        // Arrange
        await using var editorContext = BuildEditorContext();

        var request = new GetDownloadFilePreSignedUrlRequest(ObjectProvider.Create<string>(), 0, 0);

        // Act
        var handler = new GetDownloadFilePreSignedUrlRequestHandler(
            (_, _) => Task.CompletedTask,
            editorContext,
            new RoadNetworkExtractDownloadsBlobClient(Mock.Of<IBlobClient>()),
            Store,
            Clock,
            null,
            null,
            null,
            LoggerFactory
        );
        var act= () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidGuidValidationException>();
    }

    [Fact]
    public async Task WhenExtractDownloadDoesNotExist_ThenDownloadExtractNotFoundException()
    {
        // Arrange
        await using var editorContext = BuildEditorContext();

        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new GetDownloadFilePreSignedUrlRequest(downloadId, 0, 0);

        // Act
        var handler = new GetDownloadFilePreSignedUrlRequestHandler(
            (_, _) => Task.CompletedTask,
            editorContext,
            new RoadNetworkExtractDownloadsBlobClient(Mock.Of<IBlobClient>()),
            Store,
            Clock,
            null,
            null,
            null,
            LoggerFactory
        );
        var act= () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DownloadExtractNotFoundException>();
    }

    [Fact]
    public async Task WhenExtractDownloadIsNotAvailable_ThenDownloadExtractNotFoundException()
    {
        // Arrange
        await using var editorContext = BuildEditorContext();

        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new GetDownloadFilePreSignedUrlRequest(downloadId, 0, 0);

        editorContext.ExtractDownloads.Add(new ExtractDownloadRecord
        {
            DownloadId = downloadId.ToGuid(),
            ExternalRequestId = ObjectProvider.Create<string>(),
            RequestId = ObjectProvider.Create<string>(),
            Available = false
        });
        await editorContext.SaveChangesAsync();

        // Act
        var handler = new GetDownloadFilePreSignedUrlRequestHandler(
            (_, _) => Task.CompletedTask,
            editorContext,
            new RoadNetworkExtractDownloadsBlobClient(Mock.Of<IBlobClient>()),
            Store,
            Clock,
            null,
            null,
            null,
            LoggerFactory
        );
        var act= () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DownloadExtractNotFoundException>();
    }

    [Fact]
    public async Task WhenExtractDownloadArchiveIdIsEmpty_ThenExtractArchiveNotCreatedException()
    {
        // Arrange
        await using var editorContext = BuildEditorContext();

        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new GetDownloadFilePreSignedUrlRequest(downloadId, 0, 0);

        editorContext.ExtractDownloads.Add(new ExtractDownloadRecord
        {
            DownloadId = downloadId.ToGuid(),
            ExternalRequestId = ObjectProvider.Create<string>(),
            RequestId = ObjectProvider.Create<string>(),
            Available = true
        });
        await editorContext.SaveChangesAsync();

        // Act
        var handler = new GetDownloadFilePreSignedUrlRequestHandler(
            (_, _) => Task.CompletedTask,
            editorContext,
            new RoadNetworkExtractDownloadsBlobClient(Mock.Of<IBlobClient>()),
            Store,
            Clock,
            null,
            null,
            null,
            LoggerFactory
        );
        var act= () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ExtractArchiveNotCreatedException>();
    }

    [Fact]
    public async Task WhenBlobDoesNotExist_ThenBlobNotFoundException()
    {
        // Arrange
        await using var editorContext = BuildEditorContext();

        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new GetDownloadFilePreSignedUrlRequest(downloadId, 0, 0);

        editorContext.ExtractDownloads.Add(new ExtractDownloadRecord
        {
            DownloadId = downloadId.ToGuid(),
            ExternalRequestId = ObjectProvider.Create<string>(),
            RequestId = ObjectProvider.Create<string>(),
            Available = true,
            ArchiveId = ObjectProvider.Create<string>()
        });
        await editorContext.SaveChangesAsync();

        // Act
        var handler = new GetDownloadFilePreSignedUrlRequestHandler(
            (_, _) => Task.CompletedTask,
            editorContext,
            new RoadNetworkExtractDownloadsBlobClient(Mock.Of<IBlobClient>()),
            Store,
            Clock,
            null,
            null,
            null,
            LoggerFactory
        );
        var act= () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BlobNotFoundException>();
    }

    private EditorContext BuildEditorContext()
    {
        var options = new DbContextOptionsBuilder<EditorContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new EditorContext(options);
    }
}
