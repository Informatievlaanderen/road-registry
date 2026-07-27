namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenDataValidation;

using AutoFixture;
using BackOffice.Uploads;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using RoadNetwork;
using RoadRegistry.BackOffice.FeatureToggles;
using RoadRegistry.Extracts.DataValidation;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests;
using Xunit.Abstractions;

public class DataValidationPollingServiceTests : WhenDataValidationTestBase
{
    public DataValidationPollingServiceTests(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task WhenApproved_ThenQualityReportIsStoredAndUploadIsAccepted()
    {
        // Arrange
        var migrateRoadNetworkSqsRequest = ObjectProvider.Create<MigrateRoadNetworkSqsRequest>();
        const string qualityReportUrl = "https://example.org/kwaliteitsrapport.html";
        var serializer = new SqsJsonMessageSerializer(new FakeSqsOptions(), SqsJsonMessageAssemblies.Assemblies);

        ExtractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = migrateRoadNetworkSqsRequest.UploadId.ToGuid(),
            DownloadId = migrateRoadNetworkSqsRequest.DownloadId.ToGuid(),
            Status = ExtractUploadStatus.Processing,
            TicketId = ObjectProvider.Create<TicketId>(),
            UploadedOn = DateTimeOffset.Now
        });
        ExtractsDbContext.DataValidationQueue.Add(new DataValidationQueueItem
        {
            UploadId = migrateRoadNetworkSqsRequest.UploadId.ToGuid(),
            DataValidationId = "delivery-1",
            SqsRequestJson = serializer.Serialize(migrateRoadNetworkSqsRequest),
            Completed = false
        });
        await ExtractsDbContext.SaveChangesAsync();

        DataValidationApiClientMock
            .Setup(x => x.PollDeliveryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PollDeliveryResponse(ValidationJobStatus.Processed, string.Empty, null, ValidationResult.Approved, null));
        DataValidationApiClientMock
            .Setup(x => x.GetDeliveryArtifactsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetDeliveryArtifactsResponse([new DeliveryArtifact(DeliveryArtifactType.QualityReport, qualityReportUrl)]));

        var mediatorMock = new Mock<IMediator>();

        var service = new DataValidationPollingService(
            ExtractsDbContext,
            DataValidationApiClientMock.Object,
            mediatorMock.Object,
            serializer,
            TicketingMock.Object,
            new UseDataValidationFeatureToggle(true),
            new NullLoggerFactory());

        // Act
        await service.RunAsync(CancellationToken.None);

        // Assert: on an approval the quality report is kept (so the UI always shows at least one), the migrate request is
        // sent and the queue item is completed.
        var upload = ExtractsDbContext.ExtractUploads.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        upload.QualityReportUrl.Should().Be(qualityReportUrl);

        var queueItem = ExtractsDbContext.DataValidationQueue.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        queueItem.Completed.Should().BeTrue();

        mediatorMock.Verify(x => x.Send(It.IsAny<MigrateRoadNetworkSqsRequest>(), It.IsAny<CancellationToken>()), Times.Once);
    }
}
