namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts;

using System.IO.Compression;
using Abstractions.Exceptions;
using Actions.UploadExtract;
using AutoFixture;
using BackOffice.Extracts;
using BackOffice.Uploads;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using FluentAssertions;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using RoadRegistry.Infrastructure.DutchTranslations;
using RoadRegistry.Tests.AggregateTests;
using RoadRegistry.Tests.BackOffice.Extracts.DomainV2;
using TicketingService.Abstractions;
using IZipArchiveFeatureCompareTranslator = RoadRegistry.Extracts.FeatureCompare.DomainV2.IZipArchiveFeatureCompareTranslator;
using TranslatedChanges = RoadRegistry.Extracts.FeatureCompare.DomainV2.TranslatedChanges;

public class ExtractUploaderTests
{
    private ExtractsDbContext ExtractsDbContext { get; }
    private IFixture Fixture { get; }

    public ExtractUploaderTests()
    {
        ExtractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();
        Fixture = new RoadNetworkTestDataV2().Fixture;
    }

    [Fact]
    public async Task ThenSucceeded()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var uploadId = Fixture.Create<UploadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = extractRequestId,
            CurrentDownloadId = downloadId,
            Description = Fixture.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        await ProcessUpload(downloadId, uploadId: uploadId);

        // Assert
        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.LatestUploadId.Should().Be(uploadId.ToGuid());

        var extractUpload = ExtractsDbContext.ExtractUploads.Single(x => x.UploadId == uploadId);
        extractUpload.Status.Should().Be(ExtractUploadStatus.Processing);
    }

    [Fact]
    public async Task WithUnknownExtractDownload_ThenError()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();

        // Act
        var act = () => ProcessUpload(downloadId);

        // Assert
        await act.Should().ThrowAsync<ExtractDownloadNotFoundException>();
    }

    [Fact]
    public async Task GivenInformativeExtractDownload_ThenError()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now,
            IsInformative = true
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        var act = () => ProcessUpload(downloadId);

        // Assert
        await act.Should().ThrowAsync<ExtractRequestMarkedInformativeException>();
    }

    [Fact]
    public async Task GivenClosedExtractDownload_ThenError()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now,
            Closed = true
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        var act = () => ProcessUpload(downloadId);

        // Assert
        await act.Should().ThrowAsync<ExtractRequestClosedException>();
    }

    [Fact]
    public async Task GivenExtractWhichHasNotBeenDownloadedYet_ThenError()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = null
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        var act = () => ProcessUpload(downloadId);

        // Assert
        await act.Should().ThrowAsync<CanNotUploadRoadNetworkExtractChangesArchiveForUnknownDownloadException>();
    }

    [Fact]
    public async Task GivenExtractWhichIsNotTheCurrentDownload_ThenError()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = extractRequestId,
            CurrentDownloadId = Guid.NewGuid(),
            Description = Fixture.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now,
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        var act = () => ProcessUpload(downloadId);

        // Assert
        await act.Should().ThrowAsync<CanNotUploadRoadNetworkExtractChangesArchiveForSupersededDownloadException>();
    }

    [Fact]
    public async Task GivenNoBlob_ThenError()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = extractRequestId,
            CurrentDownloadId = downloadId,
            Description = Fixture.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        // Act
        var act = () => ProcessUpload(downloadId, blobClient: blobClient.Object);

        // Assert
        await act.Should().ThrowAsync<BlobNotFoundException>();
    }

    [Fact]
    public async Task WithInvalidContentType_ThenError()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = extractRequestId,
            CurrentDownloadId = downloadId,
            Description = Fixture.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new BlobObject(Fixture.Create<BlobName>(), Metadata.None, ContentType.Parse("application/text"), _ => throw new NotSupportedException()));

        // Act
        var act = () => ProcessUpload(downloadId, blobClient: blobClient.Object);

        // Assert
        await act.Should().ThrowAsync<UnsupportedMediaTypeException>();
    }

    [Fact]
    public async Task WithCorruptArchive_ThenError()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = extractRequestId,
            CurrentDownloadId = downloadId,
            Description = Fixture.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new BlobObject(Fixture.Create<BlobName>(), Metadata.None, ContentType.Parse("binary/octet-stream"), _ => throw new CorruptArchiveException()));

        // Act
        var act = () => ProcessUpload(downloadId, blobClient: blobClient.Object);

        // Assert
        await act.Should().ThrowAsync<CorruptArchiveException>();
    }

    [Fact]
    public async Task WhenExternalRequestIdIsNullAndZipArchiveValidationException_ThenOnlyError()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = extractRequestId,
            CurrentDownloadId = downloadId,
            Description = Fixture.Create<string>(),
            ExternalRequestId = null
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new BlobObject(
                Fixture.Create<BlobName>(),
                Metadata.None,
                ContentType.Parse("binary/octet-stream"),
                _ => throw new ZipArchiveValidationException(ZipArchiveProblems.None.Add(new FileError(Fixture.Create<string>(), Fixture.Create<string>())))));

        var extractUploadFailedEmailClient = new Mock<IExtractUploadFailedEmailClient>();

        // Act
        var act = () => ProcessUpload(downloadId, blobClient: blobClient.Object, extractUploadFailedEmailClient: extractUploadFailedEmailClient.Object);

        // Assert
        await act.Should().ThrowAsync<DutchValidationException>();

        extractUploadFailedEmailClient.Verify(x => x.SendAsync(It.IsAny<FailedExtractUpload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenExternalRequestIdIsNotNullAndException_ThenErrorAndMailSent()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = extractRequestId,
            CurrentDownloadId = downloadId,
            Description = Fixture.Create<string>(),
            ExternalRequestId = Fixture.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new BlobObject(
                Fixture.Create<BlobName>(),
                Metadata.None,
                ContentType.Parse("binary/octet-stream"),
                _ => throw new Exception()));

        var extractUploadFailedEmailClient = new Mock<IExtractUploadFailedEmailClient>();

        // Act
        var act = () => ProcessUpload(downloadId, blobClient: blobClient.Object, extractUploadFailedEmailClient: extractUploadFailedEmailClient.Object);

        // Assert
        await act.Should().ThrowAsync<Exception>();

        extractUploadFailedEmailClient.Verify(x => x.SendAsync(It.IsAny<FailedExtractUpload>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WhenFeatureCompareHasError_ThenDownloadRejected()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extractRequestId = Fixture.Create<ExtractRequestId>();

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = extractRequestId,
            CurrentDownloadId = downloadId,
            Description = Fixture.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = extractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var zipArchiveFeatureCompareTranslator = new Mock<IZipArchiveFeatureCompareTranslator>();
        zipArchiveFeatureCompareTranslator
            .Setup(x => x.TranslateAsync(It.IsAny<ZipArchive>(), It.IsAny<ZipArchiveMetadata>(), It.IsAny<CancellationToken>()))
            .Throws(new FakeDomainException());

        // Act
        var act = () => ProcessUpload(downloadId, zipArchiveFeatureCompareTranslator: zipArchiveFeatureCompareTranslator.Object);

        // Assert
        await act.Should().ThrowAsync<FakeDomainException>();

        var extractDownload = ExtractsDbContext.ExtractUploads.Single(x => x.DownloadId == downloadId);
        extractDownload.Status.Should().Be(ExtractUploadStatus.AutomaticValidationFailed);
    }

    private sealed class FakeDomainException : DomainException;

    private async Task ProcessUpload(
        DownloadId downloadId,
        UploadId? uploadId = null,
        IBlobClient? blobClient = null,
        IZipArchiveFeatureCompareTranslator? zipArchiveFeatureCompareTranslator = null,
        IExtractUploadFailedEmailClient? extractUploadFailedEmailClient = null)
    {
        if (blobClient is null)
        {
            var blobClientMock = new Mock<IBlobClient>();
            blobClientMock
                .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new BlobObject(new BlobName("archive.zip"),
                    Metadata.None,
                    ContentType.Parse("application/zip"),
                    _ =>
                    {
                        var archiveStream = new DomainV2ZipArchiveBuilder()
                            .BuildArchiveStream();
                        return Task.FromResult<Stream>(archiveStream);
                    }));
            blobClient = blobClientMock.Object;
        }

        var extractUploader = new ExtractUploader(
            ExtractsDbContext,
            new RoadNetworkUploadsBlobClient(blobClient),
            zipArchiveFeatureCompareTranslator ?? new FakeZipArchiveFeatureCompareTranslator(),
            extractUploadFailedEmailClient ?? new FakeExtractUploadFailedEmailClient(),
            Mock.Of<ITicketing>()
        );

        await extractUploader.ProcessUploadAndDetectChanges(downloadId, uploadId ?? Fixture.Create<UploadId>(), Fixture.Create<TicketId>(), ZipArchiveMetadata.Empty, CancellationToken.None);
    }

    private sealed class FakeZipArchiveFeatureCompareTranslator : IZipArchiveFeatureCompareTranslator
    {
        public Task<TranslatedChanges> TranslateAsync(ZipArchive archive, ZipArchiveMetadata zipArchiveMetadata, CancellationToken cancellationToken)
        {
            return Task.FromResult(TranslatedChanges.Empty);
        }
    }
}
