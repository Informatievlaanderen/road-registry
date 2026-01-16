namespace RoadRegistry.BackOffice.Handlers.Tests.Uploads.WhenUploadingExtract;

using Abstractions.Exceptions;
using Abstractions.Uploads;
using Autofac;
using AutoFixture;
using BackOffice.Framework;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Editor.Schema;
using Editor.Schema.Extracts;
using Exceptions;
using FeatureCompare;
using FluentAssertions;
using Handlers.Uploads;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.BackOffice.Extracts.V1;
using RoadRegistry.Tests.BackOffice.Scenarios;
using Xunit.Abstractions;
using ZipArchiveWriters.Cleaning;
using UploadExtractRequest = Abstractions.Uploads.UploadExtractRequest;

public class UploadExtractRequestTests: RoadNetworkTestBase
{
    private readonly TransactionZoneZipArchiveReader _transactionZoneZipArchiveReader;

    public UploadExtractRequestTests(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
        _transactionZoneZipArchiveReader = new(new(FileEncoding.UTF8), new(FileEncoding.UTF8));
    }

    [Fact]
    public async Task GivenDownload_WhenValidUpload_ThenUploadRoadNetworkChangesArchive()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var externalRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var requestId = ExtractRequestId.FromExternalRequestId(externalRequestId);
        var ticketId = ObjectProvider.Create<Guid>();
        var zipArchiveWriterVersion = WellKnownZipArchiveWriterVersions.DomainV1_2;

        await using var editorContext = Container.Resolve<EditorContext>();
        editorContext.ExtractDownloads.Add(new ExtractDownloadRecord
        {
            DownloadId = downloadId,
            RequestId = requestId,
            ExternalRequestId = externalRequestId,
            IsInformative = false,
            ZipArchiveWriterVersion = zipArchiveWriterVersion
        });
        await editorContext.SaveChangesAsync();

        using var archiveStream = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                builder.TestData.TransactionZoneDbaseRecord.DOWNLOADID.Value = downloadId;
            })
            .BuildArchiveStream();

        var request = new UploadExtractRequest(
            new UploadExtractArchiveRequest("archive.zip", archiveStream, ContentType.Parse("binary/octet-stream")),
            ticketId);

        var zipArchiveBeforeFeatureCompareValidatorFactoryMock = new Mock<IZipArchiveBeforeFeatureCompareValidatorFactory>();
        zipArchiveBeforeFeatureCompareValidatorFactoryMock
            .Setup(x => x.Create(It.IsAny<string>()))
            .Returns(ZipArchiveBeforeFeatureCompareValidator);

        // Act
        var sut = new UploadExtractRequestHandler(
            new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
            _transactionZoneZipArchiveReader,
            editorContext,
            zipArchiveBeforeFeatureCompareValidatorFactoryMock.Object,
            new RoadNetworkCommandQueue(Store, new ApplicationMetadata(RoadRegistryApplication.BackOffice)),
            Mock.Of<IBeforeFeatureCompareZipArchiveCleanerFactory>(),
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );
        var response = await sut.Handle(request, CancellationToken.None);

        var command = await Store.GetLastMessage<UploadRoadNetworkChangesArchive>();
        command.DownloadId.Should().Be(downloadId.ToGuid());
        command.ExtractRequestId.Should().Be(requestId);
        command.ArchiveId.Should().BeEquivalentTo(response.ArchiveId);
        command.ZipArchiveWriterVersion.Should().Be(zipArchiveWriterVersion);
        command.TicketId.Should().Be(ticketId);
    }

    [Fact]
    public async Task WhenInvalidContentType_ThenUnsupportedMediaTypeException()
    {
        // Arrange
        await using var editorContext = Container.Resolve<EditorContext>();

        var request = new UploadExtractRequest(
            new UploadExtractArchiveRequest("archive.zip", null!, ContentType.Parse("application/invalid_contenttype")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
            _transactionZoneZipArchiveReader,
            editorContext,
            Mock.Of<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
            Mock.Of<IRoadNetworkCommandQueue>(),
            Mock.Of<IBeforeFeatureCompareZipArchiveCleanerFactory>(),
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() =>
            sut.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task WhenUnknownDownloadId_ThenDownloadExtractNotFoundException()
    {
        // Arrange
        await using var editorContext = Container.Resolve<EditorContext>();

        using var archiveStream = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                builder.TestData.TransactionZoneDbaseRecord.DOWNLOADID.Value = ObjectProvider.Create<DownloadId>();
            })
            .BuildArchiveStream();

        var request = new UploadExtractRequest(
            new UploadExtractArchiveRequest("archive.zip", archiveStream, ContentType.Parse("binary/octet-stream")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
            _transactionZoneZipArchiveReader,
            editorContext,
            Mock.Of<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
            Mock.Of<IRoadNetworkCommandQueue>(),
            Mock.Of<IBeforeFeatureCompareZipArchiveCleanerFactory>(),
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        await Assert.ThrowsAsync<ExtractDownloadNotFoundException>(() =>
            sut.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task WhenDownloadIdIsNotAGuid_ThenDownloadIdInvalidFormat()
    {
        // Arrange
        await using var editorContext = Container.Resolve<EditorContext>();

        using var archiveStream = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                builder.TestData.TransactionZoneDbaseRecord.DOWNLOADID.Value = "abc";
            })
            .BuildArchiveStream();

        var request = new UploadExtractRequest(
            new UploadExtractArchiveRequest("archive.zip", archiveStream, ContentType.Parse("binary/octet-stream")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
            _transactionZoneZipArchiveReader,
            editorContext,
            Mock.Of<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
            Mock.Of<IRoadNetworkCommandQueue>(),
            Mock.Of<IBeforeFeatureCompareZipArchiveCleanerFactory>(),
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() =>
            sut.Handle(request, CancellationToken.None));
        ex.Problems.Single().Reason.Should().Be("DownloadIdInvalidFormat");
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

        using var archiveStream = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                builder.TestData.TransactionZoneDbaseRecord.DOWNLOADID.Value = downloadId;
            })
            .BuildArchiveStream();

        var request = new UploadExtractRequest(
            new UploadExtractArchiveRequest("archive.zip", archiveStream, ContentType.Parse("binary/octet-stream")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
            _transactionZoneZipArchiveReader,
            editorContext,
            Mock.Of<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
            Mock.Of<IRoadNetworkCommandQueue>(),
            Mock.Of<IBeforeFeatureCompareZipArchiveCleanerFactory>(),
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        await Assert.ThrowsAsync<ExtractRequestMarkedInformativeException>(() =>
            sut.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task WhenArchiveIsNotAnArchiveFile_ThenUnsupportedMediaTypeException()
    {
        // Arrange
        await using var editorContext = Container.Resolve<EditorContext>();

        var request = new UploadExtractRequest(
            new UploadExtractArchiveRequest("archive.zip", await EmbeddedResourceReader.ReadAsync("test.txt"), ContentType.Parse("binary/octet-stream")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
            _transactionZoneZipArchiveReader,
            editorContext,
            Mock.Of<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
            Mock.Of<IRoadNetworkCommandQueue>(),
            Mock.Of<IBeforeFeatureCompareZipArchiveCleanerFactory>(),
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        await Assert.ThrowsAsync<UnsupportedMediaTypeException>(() =>
            sut.Handle(request, CancellationToken.None));
    }

    [Fact]
    public async Task WhenEmptyArchive_ThenZipArchiveValidationException()
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
            new UploadExtractArchiveRequest("archive.zip", await EmbeddedResourceReader.ReadAsync("empty.zip"), ContentType.Parse("binary/octet-stream")),
            null);

        // Act
        var sut = new UploadExtractRequestHandler(
            new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
            _transactionZoneZipArchiveReader,
            editorContext,
            Mock.Of<IZipArchiveBeforeFeatureCompareValidatorFactory>(),
            Mock.Of<IRoadNetworkCommandQueue>(),
            Mock.Of<IBeforeFeatureCompareZipArchiveCleanerFactory>(),
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );

        // Assert
        var ex = await Assert.ThrowsAsync<ZipArchiveValidationException>(() =>
            sut.Handle(request, CancellationToken.None));

        var validationFileProblems = ex.Problems.Select(fileProblem => fileProblem.File).ToArray();

        Assert.Contains("TRANSACTIEZONES.DBF", validationFileProblems);
    }

    [Fact]
    public async Task WhenValidArchive_WithLaneNullVanPos_ThenUploadRoadNetworkChangesArchive()
    {
        await ProcessRequest(builder =>
        {
            builder.TestData.RoadSegment1LaneDbaseRecord.VANPOS.Value = null;
        });
    }

    [Fact]
    public async Task WhenValidArchive_WithLaneNullTotPos_ThenUploadRoadNetworkChangesArchive()
    {
        await ProcessRequest(builder =>
        {
            builder.TestData.RoadSegment1LaneDbaseRecord.TOTPOS.Value = null;
        });
    }

    [Fact]
    public async Task WhenValidArchive_WithSurfaceNullVanPos_ThenUploadRoadNetworkChangesArchive()
    {
        await ProcessRequest(builder =>
        {
            builder.TestData.RoadSegment1SurfaceDbaseRecord.VANPOS.Value = null;
        });
    }

    [Fact]
    public async Task WhenValidArchive_WithSurfaceNullTotPos_ThenUploadRoadNetworkChangesArchive()
    {
        await ProcessRequest(builder =>
        {
            builder.TestData.RoadSegment1SurfaceDbaseRecord.TOTPOS.Value = null;
        });
    }

    [Fact]
    public async Task WhenValidArchive_WithWidthNullVanPos_ThenUploadRoadNetworkChangesArchive()
    {
        await ProcessRequest(builder =>
        {
            builder.TestData.RoadSegment1WidthDbaseRecord.VANPOS.Value = null;
        });
    }

    [Fact]
    public async Task WhenValidArchive_WithWidthNullTotPos_ThenUploadRoadNetworkChangesArchive()
    {
        await ProcessRequest(builder =>
        {
            builder.TestData.RoadSegment1WidthDbaseRecord.TOTPOS.Value = null;
        });
    }

    private async Task ProcessRequest(Action<ExtractsZipArchiveChangeDataSetBuilder> configureChange = null)
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var externalRequestId = ObjectProvider.Create<ExternalExtractRequestId>();
        var requestId = ExtractRequestId.FromExternalRequestId(externalRequestId);
        var ticketId = ObjectProvider.Create<Guid>();
        var zipArchiveWriterVersion = WellKnownZipArchiveWriterVersions.DomainV1_2;

        await using var editorContext = Container.Resolve<EditorContext>();
        editorContext.ExtractDownloads.Add(new ExtractDownloadRecord
        {
            DownloadId = downloadId,
            RequestId = requestId,
            ExternalRequestId = externalRequestId,
            IsInformative = false,
            ZipArchiveWriterVersion = zipArchiveWriterVersion
        });
        await editorContext.SaveChangesAsync();

        using var archiveStream = new ExtractV1ZipArchiveBuilder()
            .WithChange((builder, _) =>
            {
                builder.TestData.TransactionZoneDbaseRecord.DOWNLOADID.Value = downloadId;

                configureChange?.Invoke(builder);
            })
            .BuildArchiveStream();

        var request = new UploadExtractRequest(
            new UploadExtractArchiveRequest("archive.zip", archiveStream, ContentType.Parse("binary/octet-stream")),
            ticketId);

        var zipArchiveBeforeFeatureCompareValidatorFactoryMock = new Mock<IZipArchiveBeforeFeatureCompareValidatorFactory>();
        zipArchiveBeforeFeatureCompareValidatorFactoryMock
            .Setup(x => x.Create(It.IsAny<string>()))
            .Returns(ZipArchiveBeforeFeatureCompareValidator);

        // Act
        var sut = new UploadExtractRequestHandler(
            new RoadNetworkUploadsBlobClient(Mock.Of<IBlobClient>()),
            _transactionZoneZipArchiveReader,
            editorContext,
            zipArchiveBeforeFeatureCompareValidatorFactoryMock.Object,
            new RoadNetworkCommandQueue(Store, new ApplicationMetadata(RoadRegistryApplication.BackOffice)),
            Mock.Of<IBeforeFeatureCompareZipArchiveCleanerFactory>(),
            Mock.Of<ILogger<UploadExtractRequestHandler>>()
        );
        await sut.Handle(request, CancellationToken.None);
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
