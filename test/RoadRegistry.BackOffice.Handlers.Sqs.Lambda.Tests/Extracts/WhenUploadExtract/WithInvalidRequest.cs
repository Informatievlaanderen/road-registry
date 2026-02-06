namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenUploadExtract;

using Abstractions.Exceptions;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.AggregateSource;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentAssertions;
using Moq;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Extracts.Uploads;
using Sqs.Extracts;
using TicketingService.Abstractions;
using Xunit.Abstractions;
using Polygon = NetTopologySuite.Geometries.Polygon;

public class WithInvalidRequest : WhenUploadExtractTestBase
{
    public WithInvalidRequest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task ThenErrorAndOriginalTicketAlsoUpdated()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = request.ExtractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .Throws(new FakeDomainException());

        // Act
        var sqsRequest = await HandleRequest(request, blobClient: blobClient.Object);

        // Assert
        TicketingMock.Verify(x =>
            x.Error(
                sqsRequest.TicketId,
                It.IsAny<TicketError>(),
                CancellationToken.None
            )
        );

        var extractUpload = ExtractsDbContext.ExtractUploads.Single(x => x.UploadId == request.UploadId.ToGuid());
        extractUpload.DownloadId.Should().Be(downloadId.ToGuid());
        extractUpload.Status.Should().Be(ExtractUploadStatus.AutomaticValidationFailed);
    }

    [Fact]
    public async Task GivenNoBlob_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = request.ExtractRequestId,
            CurrentDownloadId = downloadId,
            Description = ObjectProvider.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = request.ExtractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => null);

        // Act
        var act = () => HandleRequest(request, blobClient: blobClient.Object);

        // Assert
        await act.Should().ThrowAsync<BlobNotFoundException>();
    }

    [Fact]
    public async Task WithInvalidContentType_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = request.ExtractRequestId,
            CurrentDownloadId = downloadId,
            Description = ObjectProvider.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = request.ExtractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new BlobObject(ObjectProvider.Create<BlobName>(), Metadata.None, ContentType.Parse("application/text"), _ => throw new NotSupportedException()));

        // Act
        await HandleRequest(request, blobClient: blobClient.Object);

        // Assert
        VerifyThatTicketHasError(code: "UnsupportedMediaType", message: "Bestandstype is foutief. 'application/text' is geen geldige waarde.");
    }

    [Fact]
    public async Task WithUnknownExtractDownload_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasError(code: "ExtractNietGekend", message: "Het extract werd niet gevonden.");
    }

    [Fact]
    public async Task GivenInformativeExtractDownload_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = request.ExtractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now,
            IsInformative = true
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasError(code: "ExtractIsInformatief", message: "Upload is niet toegelaten voor een informatieve extractaanvraag.");
    }

    [Fact]
    public async Task GivenClosedExtractDownload_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = request.ExtractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now,
            Closed = true
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasError(code: "ExtractIsGesloten", message: "Upload is niet toegelaten voor een gesloten extractaanvraag.");
    }

    [Fact]
    public async Task GivenExtractWhichHasNotBeenDownloadedYet_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = request.ExtractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = null
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasError(code: "ExtractNogNietGedownload", message: "Upload is enkel toegelaten voor een extract dat minstens 1 keer is gedownload.");
    }

    [Fact]
    public async Task GivenExtractWhichIsNotTheCurrentDownload_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = request.ExtractRequestId,
            CurrentDownloadId = Guid.NewGuid(),
            Description = ObjectProvider.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = request.ExtractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now,
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasError(code: "UploadEnkelLaatsteDownload", message: "Upload is enkel toegelaten voor de laatste download van het extractaanvraag.");
    }

    [Fact]
    public async Task WithCorruptArchive_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = request.ExtractRequestId,
            CurrentDownloadId = downloadId,
            Description = ObjectProvider.Create<string>()
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = request.ExtractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new BlobObject(ObjectProvider.Create<BlobName>(), Metadata.None, ContentType.Parse("binary/octet-stream"), _ => throw new CorruptArchiveException()));

        // Act
        await HandleRequest(request, blobClient: blobClient.Object);

        // Assert
        VerifyThatTicketHasError(code: "ArchiefCorrupt", message: "Het geüploade archief is corrupt of onleesbaar.");
    }

    [Fact]
    public async Task WhenExternalRequestIdIsNullAndZipArchiveValidationException_ThenOnlyError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = request.ExtractRequestId,
            CurrentDownloadId = downloadId,
            Description = ObjectProvider.Create<string>(),
            ExternalRequestId = null
        });
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            ExtractRequestId = request.ExtractRequestId,
            Contour = Polygon.Empty,
            DownloadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        var blobClient = new Mock<IBlobClient>();
        blobClient
            .Setup(x => x.GetBlobAsync(It.IsAny<BlobName>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new BlobObject(
                ObjectProvider.Create<BlobName>(),
                Metadata.None,
                ContentType.Parse("binary/octet-stream"),
                _ => throw new ZipArchiveValidationException(ZipArchiveProblems.None.Add(new FileError(ObjectProvider.Create<string>(), ObjectProvider.Create<string>())))));

        var extractUploadFailedEmailClient = new Mock<IExtractUploadFailedEmailClient>();

        // Act
        await HandleRequest(request, blobClient: blobClient.Object, extractUploadFailedEmailClient: extractUploadFailedEmailClient.Object);

        // Assert
        TicketingMock.Verify(x =>
            x.Error(
                It.IsAny<Guid>(),
                It.IsAny<TicketError>(),
                CancellationToken.None
            )
        );

        extractUploadFailedEmailClient.Verify(x => x.SendAsync(It.IsAny<FailedExtractUpload>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenExternalRequestIdIsNotNullAndException_ThenErrorAndMailSent()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var extractRequestId = ObjectProvider.Create<ExtractRequestId>();
        var request = new UploadExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = extractRequestId
        };

        ExtractsDbContext.ExtractRequests.Add(new()
        {
            ExtractRequestId = extractRequestId,
            CurrentDownloadId = downloadId,
            Description = ObjectProvider.Create<string>(),
            ExternalRequestId = ObjectProvider.Create<string>()
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
                ObjectProvider.Create<BlobName>(),
                Metadata.None,
                ContentType.Parse("binary/octet-stream"),
                _ => throw new Exception()));

        var extractUploadFailedEmailClient = new Mock<IExtractUploadFailedEmailClient>();

        // Act
        var act = () => HandleRequest(request, blobClient: blobClient.Object, extractUploadFailedEmailClient: extractUploadFailedEmailClient.Object);

        // Assert
        await act.Should().ThrowAsync<Exception>();

        extractUploadFailedEmailClient.Verify(x => x.SendAsync(It.IsAny<FailedExtractUpload>(), It.IsAny<CancellationToken>()));
    }

    private sealed class FakeDomainException : DomainException;
}
