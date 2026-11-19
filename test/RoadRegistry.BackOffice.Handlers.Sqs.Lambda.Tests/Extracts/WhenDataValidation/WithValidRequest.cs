namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenDataValidation;

using AutoFixture;
using FluentAssertions;
using Moq;
using RoadNetwork;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.DataValidation;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Infrastructure;
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
            .Setup(x => x.RequestDataValidationAsync(It.IsAny<UploadId>(), It.IsAny<Stream>(), It.IsAny<DataValidationDeliveryMetadata>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ObjectProvider.Create<string>());

        DataValidationApiClientMock
            .Setup(x => x.PollDeliveryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new PollDeliveryResponse("Processing", string.Empty, "gischecks", string.Empty, string.Empty));

        GivenInwinningszone(migrateRoadNetworkSqsRequest);
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
            x.RequestDataValidationAsync(It.IsAny<UploadId>(), It.IsAny<Stream>(), It.IsAny<DataValidationDeliveryMetadata>(), It.IsAny<CancellationToken>()));
    }

    [Fact]
    public async Task WhenFeatureEnabled_ThenDeliveryMetadataIsSent()
    {
        // Arrange
        var migrateRoadNetworkSqsRequest = ObjectProvider.Create<MigrateRoadNetworkSqsRequest>();
        var ticketId = ObjectProvider.Create<TicketId>();
        var organizationCode = new OrganizationId("OVO002949");
        var kboNumber = new OrganizationKboNumber("0316380841");
        var nisCode = "44021";

        DataValidationDeliveryMetadata? sentMetadata = null;
        DataValidationApiClientMock
            .Setup(x => x.RequestDataValidationAsync(It.IsAny<UploadId>(), It.IsAny<Stream>(), It.IsAny<DataValidationDeliveryMetadata>(), It.IsAny<CancellationToken>()))
            .Callback<UploadId, Stream, DataValidationDeliveryMetadata, CancellationToken>((_, _, metadata, _) => sentMetadata = metadata)
            .ReturnsAsync(() => ObjectProvider.Create<string>());

        DataValidationApiClientMock
            .Setup(x => x.PollDeliveryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new PollDeliveryResponse("Processing", string.Empty, "gischecks", string.Empty, string.Empty));

        OrganizationCacheMock
            .Setup(x => x.FindByIdOrOvoCodeOrKboNumberAsync(organizationCode, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new OrganizationDetail
            {
                Code = organizationCode,
                Name = new OrganizationName("Gemeente"),
                KboNumber = kboNumber
            });

        GivenInwinningszone(migrateRoadNetworkSqsRequest, nisCode, organizationCode);
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
        sentMetadata.Should().NotBeNull();
        sentMetadata!.SurveyorOrganisationKbo.Should().Be(kboNumber.ToString());
        sentMetadata.CorrelationId.Should().Be(migrateRoadNetworkSqsRequest.DownloadId.ToString());
        sentMetadata.NisCode.Should().Be(nisCode);
    }

    [Fact]
    public async Task WhenNoInwinningszone_ThenThrows()
    {
        // Arrange
        var migrateRoadNetworkSqsRequest = ObjectProvider.Create<MigrateRoadNetworkSqsRequest>();
        var ticketId = ObjectProvider.Create<TicketId>();

        DataValidationApiClientMock
            .Setup(x => x.RequestDataValidationAsync(It.IsAny<UploadId>(), It.IsAny<Stream>(), It.IsAny<DataValidationDeliveryMetadata>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ObjectProvider.Create<string>());

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
        var act = () => HandleRequest(migrateRoadNetworkSqsRequest, ticketId: ticketId, featureEnabled: true);

        // Assert: every delivery along this path has an inwinningszone, so its absence is a bug, not a delivery we
        // quietly send on without metadata.
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"No Inwinningszone found for {migrateRoadNetworkSqsRequest.DownloadId}");

        DataValidationApiClientMock.Verify(x =>
            x.RequestDataValidationAsync(It.IsAny<UploadId>(), It.IsAny<Stream>(), It.IsAny<DataValidationDeliveryMetadata>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task WhenAlreadyProcessed_ThenFastPollingIsAbortedAndLeftForSlowPolling()
    {
        // Arrange
        var migrateRoadNetworkSqsRequest = ObjectProvider.Create<MigrateRoadNetworkSqsRequest>();
        var ticketId = ObjectProvider.Create<TicketId>();

        DataValidationApiClientMock
            .Setup(x => x.RequestDataValidationAsync(It.IsAny<UploadId>(), It.IsAny<Stream>(), It.IsAny<DataValidationDeliveryMetadata>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => ObjectProvider.Create<string>());

        DataValidationApiClientMock
            .Setup(x => x.PollDeliveryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => new PollDeliveryResponse(ValidationJobStatus.Processed, string.Empty, null, ValidationResult.Approved, null));

        GivenInwinningszone(migrateRoadNetworkSqsRequest);
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
            x.RequestDataValidationAsync(It.IsAny<UploadId>(), It.IsAny<Stream>(), It.IsAny<DataValidationDeliveryMetadata>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private void GivenInwinningszone(MigrateRoadNetworkSqsRequest migrateRoadNetworkSqsRequest, string nisCode = "44021", string organizationCode = "OVO002949")
    {
        ExtractsDbContext.Inwinningszones.Add(new Inwinningszone
        {
            NisCode = nisCode,
            Contour = ObjectProvider.Create<MultiPolygon>(),
            Operator = organizationCode,
            DownloadId = migrateRoadNetworkSqsRequest.DownloadId,
            Completed = false
        });
    }
}
