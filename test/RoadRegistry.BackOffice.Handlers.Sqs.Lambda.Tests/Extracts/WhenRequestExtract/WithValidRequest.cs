namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenRequestExtract;

using Abstractions.Extracts.V2;
using AutoFixture;
using BackOffice.Extracts;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentAssertions;
using FluentValidation;
using Framework;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.Schema;
using Xunit.Abstractions;

public class WithValidRequest : WhenRequestExtractTestBase
{
    public WithValidRequest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task WithNotInformativeRequest_ThenSucceeded()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new RequestExtractRequest(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            MultiPolygon.Empty.AsText(),
            ObjectProvider.Create<string>(),
            false,
            ObjectProvider.Create<string>()
        );

        var blobClientMock = new Mock<IBlobClient>();

        // Act
        var sqsRequest = await HandleRequest(request, blobClient: blobClientMock.Object);

        // Assert
        VerifyThatTicketHasCompleted(new RequestExtractResponse(downloadId));

        var extractRequest = ExtractsDbContext.ExtractRequests.Single(x => x.ExtractRequestId == request.ExtractRequestId);
        extractRequest.CurrentDownloadId.Should().Be(request.DownloadId);
        extractRequest.Description.Should().Be(request.Description);
        extractRequest.OrganizationCode.Should().Be(sqsRequest.ProvenanceData!.Operator);
        extractRequest.ExternalRequestId.Should().Be(request.ExternalRequestId);

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.ExtractRequestId.Should().Be(request.ExtractRequestId);
        extractDownload.Closed.Should().BeFalse();
        extractDownload.Contour.AsText().Should().Be(request.Contour);
        extractDownload.Contour.SRID.Should().NotBe(0);
        extractDownload.TicketId.Should().Be(sqsRequest.TicketId);
        extractDownload.DownloadStatus.Should().Be(ExtractDownloadStatus.Available);
        extractDownload.IsInformative.Should().Be(request.IsInformative);
        extractDownload.UploadedOn.Should().BeNull();
        extractDownload.UploadId.Should().BeNull();
        extractDownload.UploadStatus.Should().BeNull();

        blobClientMock.Verify(x => x.CreateBlobAsync(
            new BlobName(downloadId),
            Metadata.None,
            ContentType.Parse("application/x-zip-compressed"),
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WithInformativeRequest_ThenSucceededWithClosedExtract()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new RequestExtractRequest(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            MultiPolygon.Empty.AsText(),
            ObjectProvider.Create<string>(),
            true,
            ObjectProvider.Create<string>()
        );

        var blobClientMock = new Mock<IBlobClient>();

        // Act
        await HandleRequest(request, blobClient: blobClientMock.Object);

        // Assert
        VerifyThatTicketHasCompleted(new RequestExtractResponse(downloadId));

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.Closed.Should().BeTrue();
    }

    [Fact]
    public async Task GivenExtractRequest_ThenReUsedAndPreviousDownloadClosed()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new RequestExtractRequest(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            MultiPolygon.Empty.AsText(),
            ObjectProvider.Create<string>(),
            false,
            ObjectProvider.Create<string>()
        );

        var originalExtractRequest = new ExtractRequest
        {
            ExtractRequestId = request.ExtractRequestId,
            Description = ObjectProvider.Create<string>(),
            CurrentDownloadId = ObjectProvider.Create<DownloadId>(),
            OrganizationCode = ObjectProvider.Create<string>(),
            ExternalRequestId = ObjectProvider.Create<string>()
        };
        ExtractsDbContext.ExtractRequests.Add(originalExtractRequest);
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            ExtractRequestId = request.ExtractRequestId,
            DownloadId = ObjectProvider.Create<DownloadId>(),
            Contour = MultiPolygon.Empty,
            Closed = false
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        var sqsRequest = await HandleRequest(request);

        // Assert
        VerifyThatTicketHasCompleted(new RequestExtractResponse(downloadId));

        var extractRequest = ExtractsDbContext.ExtractRequests.Single(x => x.ExtractRequestId == request.ExtractRequestId);
        extractRequest.CurrentDownloadId.Should().Be(request.DownloadId);
        extractRequest.Description.Should().Be(originalExtractRequest.Description);
        extractRequest.OrganizationCode.Should().Be(originalExtractRequest.OrganizationCode);
        extractRequest.ExternalRequestId.Should().Be(originalExtractRequest.ExternalRequestId);

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.ExtractRequestId.Should().Be(request.ExtractRequestId);
        extractDownload.Closed.Should().BeFalse();
        extractDownload.Contour.AsText().Should().Be(request.Contour);
        extractDownload.TicketId.Should().Be(sqsRequest.TicketId);
        extractDownload.DownloadStatus.Should().Be(ExtractDownloadStatus.Available);
        extractDownload.IsInformative.Should().Be(request.IsInformative);
        extractDownload.UploadedOn.Should().BeNull();
        extractDownload.UploadId.Should().BeNull();
        extractDownload.UploadStatus.Should().BeNull();

        var previousDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.ExtractRequestId == originalExtractRequest.ExtractRequestId
                                                                              && x.DownloadId != downloadId);
        previousDownload.Closed.Should().BeTrue();
    }

    [Fact]
    public async Task WhenArchiveAssemblyFails_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new RequestExtractRequest(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            MultiPolygon.Empty.AsText(),
            ObjectProvider.Create<string>(),
            ObjectProvider.Create<bool>(),
            ObjectProvider.Create<string>()
        );

        var archiveAssemblerMock = new Mock<IRoadNetworkExtractArchiveAssembler>();
        archiveAssemblerMock
            .Setup(x => x.AssembleArchive(It.IsAny<RoadNetworkExtractAssemblyRequest>(), It.IsAny<CancellationToken>()))
            .Throws(new ValidationException([new(ObjectProvider.Create<string>(), "error-message")]));

        // Act
        var sqsRequest = await HandleRequest(request, archiveAssembler: archiveAssemblerMock.Object);

        // Assert
        VerifyThatTicketHasError(null!, "error-message");

        var extractRequest = ExtractsDbContext.ExtractRequests.Single(x => x.ExtractRequestId == request.ExtractRequestId);
        extractRequest.CurrentDownloadId.Should().Be(request.DownloadId);
        extractRequest.Description.Should().Be(request.Description);
        extractRequest.OrganizationCode.Should().Be(sqsRequest.ProvenanceData!.Operator);
        extractRequest.ExternalRequestId.Should().Be(request.ExternalRequestId);

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.ExtractRequestId.Should().Be(request.ExtractRequestId);
        extractDownload.Closed.Should().BeFalse();
        extractDownload.Contour.AsText().Should().Be(request.Contour);
        extractDownload.TicketId.Should().Be(sqsRequest.TicketId);
        extractDownload.DownloadStatus.Should().Be(ExtractDownloadStatus.Error);
        extractDownload.IsInformative.Should().Be(request.IsInformative);
        extractDownload.UploadedOn.Should().BeNull();
        extractDownload.UploadId.Should().BeNull();
        extractDownload.UploadStatus.Should().BeNull();
    }

    [Fact]
    public async Task WhenArchiveAssemblyHasDatabaseTimeout_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new RequestExtractRequest(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            MultiPolygon.Empty.AsText(),
            ObjectProvider.Create<string>(),
            ObjectProvider.Create<bool>(),
            ObjectProvider.Create<string>()
        );

        var archiveAssemblerMock = new Mock<IRoadNetworkExtractArchiveAssembler>();
        archiveAssemblerMock
            .Setup(x => x.AssembleArchive(It.IsAny<RoadNetworkExtractAssemblyRequest>(), It.IsAny<CancellationToken>()))
            .Throws(SqlExceptionFactory.Create(number: -2));

        // Act
        await HandleRequest(request, archiveAssembler: archiveAssemblerMock.Object);

        // Assert
        VerifyThatTicketHasError("DatabaseTimeout","Er was een probleem met de databank tijdens het aanmaken van het extract.");

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.DownloadStatus.Should().Be(ExtractDownloadStatus.Error);
    }
}
