namespace RoadRegistry.BackOffice.Api.Tests.Handlers.Extracts;

using System.Linq;
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

public class WhenGetDownloadExtractPresignedUrlRequest
{
    private readonly DbContextBuilder _dbContextBuilder;
    private readonly Fixture _fixture;

    public WhenGetDownloadExtractPresignedUrlRequest(DbContextBuilder dbContextBuilder)
    {
        _dbContextBuilder = dbContextBuilder;
        _fixture = new RoadNetworkTestData().ObjectProvider;
    }

    [Fact]
    public async Task WithValidRequest_ThenResponse()
    {
        var downloadId = _fixture.Create<DownloadId>();
        var request = new GetDownloadExtractPresignedUrlRequest(downloadId);

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = _fixture.Create<ExtractRequestId>(),
            Status = ExtractDownloadStatus.Available
        });
        await extractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.BlobExistsAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var downloadFileUrlPresigner = new Mock<IDownloadFileUrlPresigner>();
        var presignedUrl = _fixture.Create<Uri>();
        downloadFileUrlPresigner
            .Setup(x => x.CreatePresignedDownloadUrl(It.IsAny<string>(), It.IsAny<BlobName>(), It.IsAny<string>()))
            .ReturnsAsync(new CreatePresignedGetResponse(presignedUrl, _fixture.Create<string>()));

        var handler = BuildHandler(extractsDbContext, blobClient.Object, downloadFileUrlPresigner.Object);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.PresignedUrl.Should().Be(presignedUrl.ToString());

        var extractDownload = extractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.DownloadedOn.Should().NotBeNull();
    }

    [Fact]
    public async Task GivenExtractNotFound_ThenException()
    {
        var downloadId = _fixture.Create<DownloadId>();
        var request = new GetDownloadExtractPresignedUrlRequest(downloadId);

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
        var request = new GetDownloadExtractPresignedUrlRequest(downloadId);

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = _fixture.Create<ExtractRequestId>(),
            Status = extractDownloadStatus
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
    public async Task GivenBlobDoesNotExist_ThenException()
    {
        var downloadId = _fixture.Create<DownloadId>();
        var request = new GetDownloadExtractPresignedUrlRequest(downloadId);

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = _fixture.Create<ExtractRequestId>(),
            Status = ExtractDownloadStatus.Available
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

    private GetDownloadExtractPresignedUrlRequestHandler BuildHandler(
        ExtractsDbContext extractsDbContext,
        IBlobClient blobClient = null,
        IDownloadFileUrlPresigner downloadFileUrlPresigner = null
    )
    {
        return new GetDownloadExtractPresignedUrlRequestHandler(
            new FakeCommandHandlerDispatcher().Dispatcher,
            extractsDbContext,
            new RoadNetworkExtractDownloadsBlobClient(blobClient ?? Mock.Of<IBlobClient>()),
            downloadFileUrlPresigner?? Mock.Of<IDownloadFileUrlPresigner>(),
            new NullLoggerFactory()
        );
    }
}
