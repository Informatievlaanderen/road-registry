namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenDataValidation;

using AutoFixture;
using BackOffice.Uploads;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NetTopologySuite.Geometries;
using RoadNetwork;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.Extracts.DataValidation;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests;
using TicketingService.Abstractions;
using Xunit.Abstractions;

public class DataValidationPollingServiceTests : WhenDataValidationTestBase
{
    private const string QualityReportUrl = "https://example.org/kwaliteitsrapport.html";

    private readonly SqsJsonMessageSerializer _serializer = new(new FakeSqsOptions(), SqsJsonMessageAssemblies.Assemblies);
    private readonly Mock<IMediator> _mediatorMock = new();

    public DataValidationPollingServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task WhenApproved_ThenQualityReportIsStoredAndUploadIsAccepted()
    {
        // Arrange
        var migrateRoadNetworkSqsRequest = await AddDelivery();
        PollDeliveryReturns(ValidationResult.Approved);

        // Act
        await RunAsync();

        // Assert: on an approval the quality report is kept (so the UI always shows at least one), the migrate request is
        // sent and the queue item is completed.
        var upload = ExtractsDbContext.ExtractUploads.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        upload.QualityReportUrl.Should().Be(QualityReportUrl);

        var queueItem = ExtractsDbContext.DataValidationQueue.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        queueItem.Completed.Should().BeTrue();

        _mediatorMock.Verify(x => x.Send(It.IsAny<MigrateRoadNetworkSqsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WhenRejected_ThenTheUploadFailsButPollingContinues()
    {
        // Arrange
        var migrateRoadNetworkSqsRequest = await AddDelivery();
        PollDeliveryReturns(ValidationResult.Rejected);

        // Act
        await RunAsync();

        // Assert: the uploader is told the delivery was rejected, but Datavalidatie can still reopen it, so the queue item
        // stays open to be polled again.
        var upload = ExtractsDbContext.ExtractUploads.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        upload.Status.Should().Be(ExtractUploadStatus.ManualValidationFailed);
        upload.QualityReportUrl.Should().Be(QualityReportUrl);

        var queueItem = ExtractsDbContext.DataValidationQueue.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        queueItem.Completed.Should().BeFalse();
        queueItem.RejectedOn.Should().NotBeNull();

        TicketingMock.Verify(x => x.Error(migrateRoadNetworkSqsRequest.TicketId, It.IsAny<TicketError>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<MigrateRoadNetworkSqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenStillRejected_ThenTheRejectionIsNotReportedAgain()
    {
        // Arrange
        var migrateRoadNetworkSqsRequest = await AddDelivery();
        PollDeliveryReturns(ValidationResult.Rejected);

        // Act: the hourly job runs again while the delivery is still rejected.
        await RunAsync();
        await RunAsync();

        // Assert: the uploader is not told the same thing twice, and we keep polling.
        TicketingMock.Verify(x => x.Error(migrateRoadNetworkSqsRequest.TicketId, It.IsAny<TicketError>(), It.IsAny<CancellationToken>()), Times.Once);

        var queueItem = ExtractsDbContext.DataValidationQueue.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        queueItem.Completed.Should().BeFalse();
    }

    [Fact]
    public async Task WhenApprovedAfterARejection_ThenTheUploadIsAcceptedAfterAll()
    {
        // Arrange: a delivery that was rejected an hour ago and has been reopened and approved by Datavalidatie since.
        var migrateRoadNetworkSqsRequest = await AddDelivery(
            uploadStatus: ExtractUploadStatus.ManualValidationFailed,
            rejectedOn: DateTimeOffset.UtcNow.AddHours(-1));
        PollDeliveryReturns(ValidationResult.Approved);

        // Act
        await RunAsync();

        // Assert: the upload and its ticket are put back where an approval expects to find them, the migrate request is
        // sent and the queue item is completed.
        var upload = ExtractsDbContext.ExtractUploads.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        upload.Status.Should().Be(ExtractUploadStatus.AutomaticValidationSucceeded);
        upload.QualityReportUrl.Should().Be(QualityReportUrl);

        var queueItem = ExtractsDbContext.DataValidationQueue.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        queueItem.Completed.Should().BeTrue();

        TicketingMock.Verify(x => x.Pending(migrateRoadNetworkSqsRequest.TicketId, It.IsAny<TicketResult>(), It.IsAny<CancellationToken>()), Times.Once);
        _mediatorMock.Verify(x => x.Send(It.IsAny<MigrateRoadNetworkSqsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WhenAutomaticallyRejected_ThenPollingStops()
    {
        // Arrange
        var migrateRoadNetworkSqsRequest = await AddDelivery();
        PollDeliveryReturns(ValidationResult.AutomaticallyRejected);

        // Act
        await RunAsync();

        // Assert: an automatic rejection is not reopened by Datavalidatie, so there is nothing left to wait for.
        var queueItem = ExtractsDbContext.DataValidationQueue.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        queueItem.Completed.Should().BeTrue();

        TicketingMock.Verify(x => x.Error(migrateRoadNetworkSqsRequest.TicketId, It.IsAny<TicketError>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(false)] // asking for a new upload URL already clears the latest upload of the extract
    [InlineData(true)] // and the new upload takes its place once it has landed
    public async Task WhenANewUploadWasStarted_ThenPollingStops(bool theNewUploadHasLanded)
    {
        // Arrange: the uploader did not wait for the rejected delivery to be reopened but started a new upload instead.
        var migrateRoadNetworkSqsRequest = await AddDelivery(
            uploadStatus: ExtractUploadStatus.ManualValidationFailed,
            rejectedOn: DateTimeOffset.UtcNow.AddHours(-1));
        await SetLatestUploadOfTheExtract(migrateRoadNetworkSqsRequest, theNewUploadHasLanded ? Guid.NewGuid() : null);
        PollDeliveryReturns(ValidationResult.Approved);

        // Act
        await RunAsync();

        // Assert: the delivery behind the previous upload no longer interests anyone, so it is not even polled.
        var queueItem = ExtractsDbContext.DataValidationQueue.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        queueItem.Completed.Should().BeTrue();

        DataValidationApiClientMock.Verify(x => x.PollDeliveryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        _mediatorMock.Verify(x => x.Send(It.IsAny<MigrateRoadNetworkSqsRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private async Task<MigrateRoadNetworkSqsRequest> AddDelivery(
        ExtractUploadStatus uploadStatus = ExtractUploadStatus.AutomaticValidationSucceeded,
        DateTimeOffset? rejectedOn = null)
    {
        var migrateRoadNetworkSqsRequest = ObjectProvider.Create<MigrateRoadNetworkSqsRequest>();

        ExtractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = migrateRoadNetworkSqsRequest.DownloadId.ToGuid(),
            ExtractRequestId = ObjectProvider.Create<string>(),
            Contour = Polygon.Empty,
            RequestedOn = DateTimeOffset.Now,
            LatestUploadId = migrateRoadNetworkSqsRequest.UploadId.ToGuid()
        });
        ExtractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = migrateRoadNetworkSqsRequest.UploadId.ToGuid(),
            DownloadId = migrateRoadNetworkSqsRequest.DownloadId.ToGuid(),
            Status = uploadStatus,
            TicketId = migrateRoadNetworkSqsRequest.TicketId,
            UploadedOn = DateTimeOffset.Now
        });
        ExtractsDbContext.DataValidationQueue.Add(new DataValidationQueueItem
        {
            UploadId = migrateRoadNetworkSqsRequest.UploadId.ToGuid(),
            DataValidationId = "delivery-1",
            SqsRequestJson = _serializer.Serialize(migrateRoadNetworkSqsRequest),
            Completed = false,
            RejectedOn = rejectedOn
        });
        await ExtractsDbContext.SaveChangesAsync();

        return migrateRoadNetworkSqsRequest;
    }

    private async Task SetLatestUploadOfTheExtract(MigrateRoadNetworkSqsRequest migrateRoadNetworkSqsRequest, Guid? latestUploadId)
    {
        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == migrateRoadNetworkSqsRequest.DownloadId.ToGuid());
        extractDownload.LatestUploadId = latestUploadId;
        await ExtractsDbContext.SaveChangesAsync();
    }

    private void PollDeliveryReturns(string validationResult)
    {
        DataValidationApiClientMock
            .Setup(x => x.PollDeliveryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PollDeliveryResponse(ValidationJobStatus.Processed, string.Empty, null, validationResult, null));
        DataValidationApiClientMock
            .Setup(x => x.GetDeliveryArtifactsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetDeliveryArtifactsResponse([new DeliveryArtifact(DeliveryArtifactType.QualityReport, QualityReportUrl)]));
    }

    private async Task RunAsync()
    {
        var service = new DataValidationPollingService(
            ExtractsDbContext,
            DataValidationApiClientMock.Object,
            _mediatorMock.Object,
            _serializer,
            TicketingMock.Object,
            new UseDataValidationFeatureToggle(true),
            new NullLoggerFactory());

        await service.RunAsync(CancellationToken.None);
    }
}
