namespace RoadRegistry.BackOffice.Handlers.Tests.Extracts.WhenUploadingExtract;

using Abstractions.Exceptions;
using Abstractions.Uploads;
using Autofac;
using AutoFixture;
using BackOffice.Extracts;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Editor.Schema;
using Editor.Schema.Extracts;
using Exceptions;
using FluentAssertions;
using Handlers.Extracts;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Xunit.Abstractions;
using UploadExtractRequest = Abstractions.Extracts.UploadExtractRequest;

public class UploadExtractRequestTests: RoadNetworkTestBase
{
    public UploadExtractRequestTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task WhenInvalidContentType_ThenUnsupportedMediaTypeException()
    {
        // Arrange
        await using var editorContext = Container.Resolve<EditorContext>();

        var request = new UploadExtractRequest(
            ObjectProvider.Create<DownloadId>(),
            new UploadExtractArchiveRequest("archive.zip", null!, ContentType.Parse("application/invalid_contenttype")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            null,
            new RoadNetworkExtractUploadsBlobClient(Mock.Of<IBlobClient>()),
            editorContext,
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        await Xunit.Assert.ThrowsAsync<UnsupportedMediaTypeException>(() =>
            sut.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task WhenNoDownloadId_ThenDownloadExtractNotFoundException()
    {
        // Arrange
        await using var editorContext = Container.Resolve<EditorContext>();

        var request = new UploadExtractRequest(
            null,
            new UploadExtractArchiveRequest("archive.zip", null!, ContentType.Parse("binary/octet-stream")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            null,
            new RoadNetworkExtractUploadsBlobClient(Mock.Of<IBlobClient>()),
            editorContext,
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        await Xunit.Assert.ThrowsAsync<DownloadExtractNotFoundException>(() =>
            sut.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task WhenDownloadIdIsNotAGuid_ThenInvalidGuidValidationException()
    {
        // Arrange
        await using var editorContext = Container.Resolve<EditorContext>();

        var request = new UploadExtractRequest(
            "abc",
            new UploadExtractArchiveRequest("archive.zip", null!, ContentType.Parse("binary/octet-stream")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            null,
            new RoadNetworkExtractUploadsBlobClient(Mock.Of<IBlobClient>()),
            editorContext,
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        await Xunit.Assert.ThrowsAsync<InvalidGuidValidationException>(() =>
            sut.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task GivenNoExtractDownload_ThenExtractDownloadNotFoundException()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();

        await using var editorContext = Container.Resolve<EditorContext>();

        var request = new UploadExtractRequest(
            downloadId,
            new UploadExtractArchiveRequest("archive.zip", null!, ContentType.Parse("binary/octet-stream")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            null,
            new RoadNetworkExtractUploadsBlobClient(Mock.Of<IBlobClient>()),
            editorContext,
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        await Xunit.Assert.ThrowsAsync<ExtractDownloadNotFoundException>(() =>
            sut.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task GivenExtractUpload_ThenCanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var externalRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var requestId = ExtractRequestId.FromExternalRequestId(externalRequestId);

        await using var editorContext = Container.Resolve<EditorContext>();
        editorContext.ExtractDownloads.Add(new ExtractDownloadRecord
        {
            DownloadId = downloadId,
            RequestId = requestId,
            ExternalRequestId = externalRequestId,
            IsInformative = true
        });
        editorContext.ExtractUploads.Add(new ExtractUploadRecord
        {
            DownloadId = downloadId,
            RequestId = requestId,
            ExternalRequestId = externalRequestId,
            ArchiveId = ObjectProvider.Create<ArchiveId>(),
            ChangeRequestId = requestId
        });
        await editorContext.SaveChangesAsync();

        var request = new UploadExtractRequest(
            downloadId,
            new UploadExtractArchiveRequest("archive.zip", null!, ContentType.Parse("binary/octet-stream")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            null,
            new RoadNetworkExtractUploadsBlobClient(Mock.Of<IBlobClient>()),
            editorContext,
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        await Xunit.Assert.ThrowsAsync<CanNotUploadRoadNetworkExtractChangesArchiveForSameDownloadMoreThanOnceException>(() =>
            sut.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task GivenInformativeDownload_ThenExtractRequestMarkedInformativeException()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var externalRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var requestId = ExtractRequestId.FromExternalRequestId(externalRequestId);

        await using var editorContext = Container.Resolve<EditorContext>();
        editorContext.ExtractDownloads.Add(new ExtractDownloadRecord
        {
            DownloadId = downloadId,
            RequestId = requestId,
            ExternalRequestId = externalRequestId,
            IsInformative = true
        });
        await editorContext.SaveChangesAsync();

        var request = new UploadExtractRequest(
            downloadId,
            new UploadExtractArchiveRequest("archive.zip", null!, ContentType.Parse("binary/octet-stream")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            null,
            new RoadNetworkExtractUploadsBlobClient(Mock.Of<IBlobClient>()),
            editorContext,
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        await Xunit.Assert.ThrowsAsync<ExtractRequestMarkedInformativeException>(() =>
            sut.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task WhenArchiveIsNotAnArchiveFile_ThenUnsupportedMediaTypeException()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var externalRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var requestId = ExtractRequestId.FromExternalRequestId(externalRequestId);

        await using var editorContext = Container.Resolve<EditorContext>();
        editorContext.ExtractDownloads.Add(new ExtractDownloadRecord
        {
            DownloadId = downloadId,
            RequestId = requestId,
            ExternalRequestId = externalRequestId,
            IsInformative = false
        });
        await editorContext.SaveChangesAsync();

        var request = new UploadExtractRequest(
            downloadId,
            new UploadExtractArchiveRequest("archive.zip", await EmbeddedResourceReader.ReadAsync("test.txt"), ContentType.Parse("binary/octet-stream")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            null,
            new RoadNetworkExtractUploadsBlobClient(Mock.Of<IBlobClient>()),
            editorContext,
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        await Xunit.Assert.ThrowsAsync<UnsupportedMediaTypeException>(() =>
            sut.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task GivenDownload_WhenValidUpload_ThenUploadRoadNetworkExtractChangesArchive()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var externalRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var requestId = ExtractRequestId.FromExternalRequestId(externalRequestId);
        var ticketId = ObjectProvider.Create<Guid>();

        await using var editorContext = Container.Resolve<EditorContext>();
        editorContext.ExtractDownloads.Add(new ExtractDownloadRecord
        {
            DownloadId = downloadId,
            RequestId = requestId,
            ExternalRequestId = externalRequestId,
            IsInformative = false
        });
        await editorContext.SaveChangesAsync();

        var request = new UploadExtractRequest(
            downloadId,
            new UploadExtractArchiveRequest("archive.zip", await EmbeddedResourceReader.ReadAsync("empty.zip"), ContentType.Parse("binary/octet-stream")),
            ticketId);

        // Act
        var sut = new UploadExtractRequestHandler(
            new RoadNetworkCommandQueue(Store, new ApplicationMetadata(RoadRegistryApplication.BackOffice)).WriteAsync,
            new RoadNetworkExtractUploadsBlobClient(Mock.Of<IBlobClient>()),
            editorContext,
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );
        var response = await sut.Handle(request, CancellationToken.None);

        // Assert
        var command = await Store.GetLastMessage<UploadRoadNetworkExtractChangesArchive>();
        command.DownloadId.Should().Be(downloadId.ToGuid());
        command.RequestId.Should().Be(requestId);
        command.UploadId.Should().Be(response.UploadId);
        command.TicketId.Should().Be(ticketId);
    }

    protected override void ConfigureContainer(ContainerBuilder containerBuilder)
    {
        base.ConfigureContainer(containerBuilder);

        containerBuilder.RegisterInstance(BuildEditorContext());
    }

    private EditorContext BuildEditorContext()
    {
        var options = new DbContextOptionsBuilder<EditorContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new EditorContext(options);
    }
}
