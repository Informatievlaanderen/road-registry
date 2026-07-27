namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenDataValidation;

using AutoFixture;
using FluentAssertions;
using Moq;
using RoadNetwork;
using RoadRegistry.Extracts.DataValidation;
using RoadRegistry.Extracts.Schema;
using Xunit.Abstractions;

public class WithValidRequest : WhenDataValidationTestBase
{
    public WithValidRequest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task WhenFeatureEnabled_ThenApiCalled()
    {
        // Arrange
        var migrateRoadNetworkSqsRequest = ObjectProvider.Create<MigrateRoadNetworkSqsRequest>();
        var ticketId = ObjectProvider.Create<TicketId>();

        DataValidationApiClientMock
            .Setup(x => x.RequestDataValidationAsync(It.IsAny<UploadId>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ObjectProvider.Create<string>());

        DataValidationApiClientMock
            .Setup(x => x.PollDeliveryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new PollDeliveryResponse("Processing", string.Empty, "gischecks", string.Empty, string.Empty));

        ExtractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = migrateRoadNetworkSqsRequest.UploadId,
            DownloadId = migrateRoadNetworkSqsRequest.DownloadId,
            Status = ExtractUploadStatus.Processing,
            TicketId = ticketId,
            UploadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(migrateRoadNetworkSqsRequest, ticketId: ticketId, featureEnabled: true);

        // Assert
        var dataValidationQueueItem = ExtractsDbContext.DataValidationQueue.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        dataValidationQueueItem.SqsRequestJson.Should().NotBeNull();
        dataValidationQueueItem.DataValidationId.Should().NotBeNull();

        DataValidationApiClientMock.Verify(x =>
            x.RequestDataValidationAsync(It.IsAny<UploadId>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WhenAlreadyProcessed_ThenFastPollingIsAbortedAndLeftForSlowPolling()
    {
        // Arrange
        var migrateRoadNetworkSqsRequest = ObjectProvider.Create<MigrateRoadNetworkSqsRequest>();
        var ticketId = ObjectProvider.Create<TicketId>();

        DataValidationApiClientMock
            .Setup(x => x.RequestDataValidationAsync(It.IsAny<UploadId>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ObjectProvider.Create<string>());

        DataValidationApiClientMock
            .Setup(x => x.PollDeliveryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new PollDeliveryResponse(ValidationJobStatus.Processed, string.Empty, null, ValidationResult.Approved, null));

        ExtractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = migrateRoadNetworkSqsRequest.UploadId,
            DownloadId = migrateRoadNetworkSqsRequest.DownloadId,
            Status = ExtractUploadStatus.Processing,
            TicketId = ticketId,
            UploadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(migrateRoadNetworkSqsRequest, ticketId: ticketId, featureEnabled: true);

        // Assert: fast polling did not decide the outcome; the queue item stays open for the slow polling service.
        var queueItem = ExtractsDbContext.DataValidationQueue.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        queueItem.Completed.Should().BeFalse();

        var upload = ExtractsDbContext.ExtractUploads.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        upload.Status.Should().Be(ExtractUploadStatus.Processing);

        DataValidationApiClientMock.Verify(x =>
            x.GetDeliveryArtifactsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WhenFeatureDisabled_ThenNoApiCalled()
    {
        // Arrange
        var migrateRoadNetworkSqsRequest = ObjectProvider.Create<MigrateRoadNetworkSqsRequest>();
        var ticketId = ObjectProvider.Create<TicketId>();

        ExtractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = migrateRoadNetworkSqsRequest.UploadId,
            DownloadId = migrateRoadNetworkSqsRequest.DownloadId,
            Status = ExtractUploadStatus.Processing,
            TicketId = ticketId,
            UploadedOn = DateTimeOffset.Now
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(migrateRoadNetworkSqsRequest, ticketId: ticketId);

        // Assert
        var dataValidationQueueItem = ExtractsDbContext.DataValidationQueue.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        dataValidationQueueItem.SqsRequestJson.Should().NotBeNull();
        dataValidationQueueItem.DataValidationId.Should().NotBeNull();

        DataValidationApiClientMock.Verify(x =>
            x.RequestDataValidationAsync(It.IsAny<UploadId>(), It.IsAny<Stream>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
