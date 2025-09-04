namespace RoadRegistry.BackOffice.Api.Tests.Handlers.Extracts;

using System.Collections.Generic;
using System.IO;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Api.Handlers.Extracts;
using RoadRegistry.BackOffice.Api.Infrastructure;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice.Scenarios;

public class WhenGetDownloadUploadPresignedUrlRequest
{
    private readonly DbContextBuilder _dbContextBuilder;
    private readonly Fixture _fixture;

    public WhenGetDownloadUploadPresignedUrlRequest(DbContextBuilder dbContextBuilder)
    {
        _dbContextBuilder = dbContextBuilder;
        _fixture = new RoadNetworkTestData().ObjectProvider;
    }

    [Fact]
    public async Task WithValidRequest_ThenResponse()
    {
        var downloadId = _fixture.Create<DownloadId>();
        var request = new GetDownloadUploadPresignedUrlRequest(downloadId);

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = _fixture.Create<ExtractRequestId>(),
            DownloadStatus = ExtractDownloadStatus.Available,
            UploadId = Guid.NewGuid()
        });
        await extractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.BlobExistsAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        blobClient
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BlobObject(new BlobName("archive.zip"),
                Metadata.None.Add(new KeyValuePair<MetadataKey, string>(new MetadataKey("filename"), _fixture.Create<string>())),
                ContentType.Parse("application/octet-stream"),
                async _ => new MemoryStream()));

        var downloadFileUrlPresigner = new Mock<IDownloadFileUrlPresigner>();
        var presignedUrl = _fixture.Create<Uri>();
        var fileName = _fixture.Create<string>();
        downloadFileUrlPresigner
            .Setup(x => x.CreatePresignedDownloadUrl(It.IsAny<string>(), It.IsAny<BlobName>(), It.IsAny<string>()))
            .ReturnsAsync(new CreatePresignedGetResponse(presignedUrl, fileName));

        var handler = BuildHandler(extractsDbContext, blobClient.Object, downloadFileUrlPresigner.Object);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.PresignedUrl.Should().Be(presignedUrl.ToString());
        response.FileName.Should().Be(fileName);
    }

    [Fact]
    public async Task GivenExtractNotFound_ThenException()
    {
        var downloadId = _fixture.Create<DownloadId>();
        var request = new GetDownloadUploadPresignedUrlRequest(downloadId);

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        var blobClient = new Mock<IBlobClient>();
        var downloadFileUrlPresigner = new Mock<IDownloadFileUrlPresigner>();

        var handler = BuildHandler(extractsDbContext, blobClient.Object, downloadFileUrlPresigner.Object);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ExtractDownloadNotFoundException>();
    }

    [Theory]
    [InlineData(ExtractDownloadStatus.Preparing)]
    [InlineData(ExtractDownloadStatus.Error)]
    public async Task GivenExtractDownloadStatusIsNotAvailable_ThenException(ExtractDownloadStatus extractDownloadStatus)
    {
        var downloadId = _fixture.Create<DownloadId>();
        var request = new GetDownloadUploadPresignedUrlRequest(downloadId);

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = _fixture.Create<ExtractRequestId>(),
            DownloadStatus = extractDownloadStatus
        });
        await extractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        var downloadFileUrlPresigner = new Mock<IDownloadFileUrlPresigner>();

        var handler = BuildHandler(extractsDbContext, blobClient.Object, downloadFileUrlPresigner.Object);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ExtractDownloadNotFoundException>();
    }

    [Fact]
    public async Task GivenExtractUploadIdIsNull_ThenException()
    {
        var downloadId = _fixture.Create<DownloadId>();
        var request = new GetDownloadUploadPresignedUrlRequest(downloadId);

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = _fixture.Create<ExtractRequestId>(),
            DownloadStatus = ExtractDownloadStatus.Available,
            UploadId = null
        });
        await extractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        var downloadFileUrlPresigner = new Mock<IDownloadFileUrlPresigner>();

        var handler = BuildHandler(extractsDbContext, blobClient.Object, downloadFileUrlPresigner.Object);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ExtractUploadNotFoundException>();
    }

    [Fact]
    public async Task GivenBlobDoesNotExist_ThenException()
    {
        var downloadId = _fixture.Create<DownloadId>();
        var request = new GetDownloadUploadPresignedUrlRequest(downloadId);

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = _fixture.Create<ExtractRequestId>(),
            DownloadStatus = ExtractDownloadStatus.Available,
            UploadId = Guid.NewGuid()
        });
        await extractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.BlobExistsAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var downloadFileUrlPresigner = new Mock<IDownloadFileUrlPresigner>();

        var handler = BuildHandler(extractsDbContext, blobClient.Object, downloadFileUrlPresigner.Object);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<BlobNotFoundException>();
    }

    private GetDownloadUploadPresignedUrlRequestHandler BuildHandler(
        ExtractsDbContext extractsDbContext,
        IBlobClient blobClient = null,
        IDownloadFileUrlPresigner downloadFileUrlPresigner = null
    )
    {
        return new GetDownloadUploadPresignedUrlRequestHandler(
            new FakeCommandHandlerDispatcher().Dispatcher,
            extractsDbContext,
            new RoadNetworkExtractUploadsBlobClient(blobClient ?? Mock.Of<IBlobClient>()),
            downloadFileUrlPresigner?? Mock.Of<IDownloadFileUrlPresigner>(),
            new NullLoggerFactory()
        );
    }
}
