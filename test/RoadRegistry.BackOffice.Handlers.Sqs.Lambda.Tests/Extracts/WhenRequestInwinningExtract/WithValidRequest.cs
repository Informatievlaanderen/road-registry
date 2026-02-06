namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenRequestInwinningExtract;

using AutoFixture;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentAssertions;
using FluentValidation;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Extracts;
using RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Framework;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Schema;
using Xunit.Abstractions;

public class WithValidRequest : WhenRequestInwinningExtractTestBase
{
    public WithValidRequest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task WithNotInformativeRequest_ThenSucceeded()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new RequestExtractData(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            MultiPolygon.Empty,
            ObjectProvider.Create<string>(),
            false,
            null
        );
        var nisCode = ObjectProvider.Create<string>();

        var blobClientMock = new Mock<IBlobClient>();

        // Act
        var sqsRequest = await HandleRequest(request, nisCode: nisCode, blobClient: blobClientMock.Object);

        // Assert
        VerifyThatTicketHasCompleted(new RequestExtractResponse(downloadId));

        var extractRequest = ExtractsDbContext.ExtractRequests.Single(x => x.ExtractRequestId == request.ExtractRequestId);
        extractRequest.CurrentDownloadId.Should().Be(request.DownloadId.ToGuid());
        extractRequest.Description.Should().Be(request.Description);
        extractRequest.OrganizationCode.Should().Be(sqsRequest.ProvenanceData!.Operator);
        extractRequest.ExternalRequestId.Should().Be($"INWINNING_{nisCode}");

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.ExtractRequestId.Should().Be(request.ExtractRequestId);
        extractDownload.Closed.Should().BeFalse();
        extractDownload.Contour.Should().BeEquivalentTo(request.Contour);
        extractDownload.TicketId.Should().Be(sqsRequest.TicketId);
        extractDownload.Status.Should().Be(ExtractDownloadStatus.Available);
        extractDownload.IsInformative.Should().Be(request.IsInformative);

        blobClientMock.Verify(x => x.CreateBlobAsync(
            new BlobName(downloadId),
            Metadata.None,
            ContentType.Parse("application/x-zip-compressed"),
            It.IsAny<Stream>(),
            It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WithInformativeRequest_ThenSucceededWithClosedExtractAndNoInwinningszoneCreated()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new RequestExtractData(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            MultiPolygon.Empty,
            ObjectProvider.Create<string>(),
            true,
            ObjectProvider.Create<string>()
        );

        var blobClientMock = new Mock<IBlobClient>();
        var nisCode = ObjectProvider.Create<string>();

        // Act
        await HandleRequest(request, nisCode: nisCode, blobClient: blobClientMock.Object);

        // Assert
        VerifyThatTicketHasCompleted(new RequestExtractResponse(downloadId));

        var extractRequest = ExtractsDbContext.ExtractRequests.Single(x => x.ExtractRequestId == request.ExtractRequestId);
        extractRequest.ExternalRequestId.Should().Be($"INWINNING_{nisCode}");

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.Closed.Should().BeTrue();

        var inwinningszone = ExtractsDbContext.Inwinningszones.SingleOrDefault(x => x.NisCode == nisCode);
        inwinningszone.Should().BeNull();
    }

    [Fact]
    public async Task GivenExtractRequest_ThenReUsedAndPreviousDownloadClosed()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new RequestExtractData(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            MultiPolygon.Empty,
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
        extractRequest.CurrentDownloadId.Should().Be(request.DownloadId.ToGuid());
        extractRequest.Description.Should().Be(originalExtractRequest.Description);
        extractRequest.OrganizationCode.Should().Be(originalExtractRequest.OrganizationCode);
        extractRequest.ExternalRequestId.Should().Be(originalExtractRequest.ExternalRequestId);

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.ExtractRequestId.Should().Be(request.ExtractRequestId);
        extractDownload.Closed.Should().BeFalse();
        extractDownload.Contour.Should().BeEquivalentTo(request.Contour);
        extractDownload.TicketId.Should().Be(sqsRequest.TicketId);
        extractDownload.Status.Should().Be(ExtractDownloadStatus.Available);
        extractDownload.IsInformative.Should().Be(request.IsInformative);

        var previousDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.ExtractRequestId == originalExtractRequest.ExtractRequestId
                                                                                                           && x.DownloadId != downloadId);
        previousDownload.Closed.Should().BeTrue();
    }

    [Fact]
    public async Task WhenPreviousLambdaRunArchiveAssemblyFailed_ThenDownloadIsNotClosed()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var ticketId = ObjectProvider.Create<TicketId>();
        var request = new RequestExtractData(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            MultiPolygon.Empty,
            ObjectProvider.Create<string>(),
            false,
            ObjectProvider.Create<string>()
        );

        var originalExtractRequest = new ExtractRequest
        {
            ExtractRequestId = request.ExtractRequestId,
            Description = request.Description,
            CurrentDownloadId = downloadId,
            OrganizationCode = ObjectProvider.Create<string>(),
            ExternalRequestId = request.ExternalRequestId
        };
        ExtractsDbContext.ExtractRequests.Add(originalExtractRequest);
        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            ExtractRequestId = request.ExtractRequestId,
            DownloadId = downloadId,
            Contour = MultiPolygon.Empty,
            IsInformative = request.IsInformative,
            Closed = false,
            TicketId = ticketId
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        var sqsRequest = await HandleRequest(request, ticketId: ticketId);

        // Assert
        VerifyThatTicketHasCompleted(new RequestExtractResponse(downloadId));

        var extractRequest = ExtractsDbContext.ExtractRequests.Single(x => x.ExtractRequestId == request.ExtractRequestId);
        extractRequest.CurrentDownloadId.Should().Be(request.DownloadId.ToGuid());
        extractRequest.Description.Should().Be(originalExtractRequest.Description);
        extractRequest.OrganizationCode.Should().Be(originalExtractRequest.OrganizationCode);
        extractRequest.ExternalRequestId.Should().Be(originalExtractRequest.ExternalRequestId);

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.ExtractRequestId.Should().Be(request.ExtractRequestId);
        extractDownload.Closed.Should().BeFalse();
        extractDownload.Contour.Should().BeEquivalentTo(request.Contour);
        extractDownload.TicketId.Should().Be(sqsRequest.TicketId);
        extractDownload.Status.Should().Be(ExtractDownloadStatus.Available);
        extractDownload.IsInformative.Should().Be(request.IsInformative);
    }

    [Fact]
    public async Task WhenArchiveAssemblyFails_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new RequestExtractData(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            MultiPolygon.Empty,
            ObjectProvider.Create<string>(),
            false,
            null
        );
        var nisCode = ObjectProvider.Create<string>();

        var archiveAssemblerMock = new Mock<IRoadNetworkExtractArchiveAssembler>();
        archiveAssemblerMock
            .Setup(x => x.AssembleArchive(It.IsAny<RoadNetworkExtractAssemblyRequest>(), It.IsAny<CancellationToken>()))
            .Throws(new ValidationException([new(ObjectProvider.Create<string>(), "error-message")]));

        // Act
        var sqsRequest = await HandleRequest(request, nisCode: nisCode, archiveAssembler: archiveAssemblerMock.Object);

        // Assert
        VerifyThatTicketHasError(null!, "error-message");

        var extractRequest = ExtractsDbContext.ExtractRequests.Single(x => x.ExtractRequestId == request.ExtractRequestId);
        extractRequest.CurrentDownloadId.Should().Be(request.DownloadId.ToGuid());
        extractRequest.Description.Should().Be(request.Description);
        extractRequest.OrganizationCode.Should().Be(sqsRequest.ProvenanceData!.Operator);
        extractRequest.ExternalRequestId.Should().Be($"INWINNING_{nisCode}");

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.ExtractRequestId.Should().Be(request.ExtractRequestId);
        extractDownload.Closed.Should().BeFalse();
        extractDownload.Contour.Should().BeEquivalentTo(request.Contour);
        extractDownload.TicketId.Should().Be(sqsRequest.TicketId);
        extractDownload.Status.Should().Be(ExtractDownloadStatus.Error);
        extractDownload.IsInformative.Should().Be(request.IsInformative);
    }

    [Fact]
    public async Task WhenArchiveAssemblyHasDatabaseTimeout_ThenError()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new RequestExtractData(
            ObjectProvider.Create<ExtractRequestId>(),
            downloadId,
            MultiPolygon.Empty,
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
        extractDownload.Status.Should().Be(ExtractDownloadStatus.Error);
    }
}
