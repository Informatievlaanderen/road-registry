namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenDataValidation;

using AutoFixture;
using FluentAssertions;
using Moq;
using RoadNetwork;
using RoadRegistry.Extracts.Schema;
using Xunit.Abstractions;

public class WithValidRequest : WhenDataValidationTestBase
{
    public WithValidRequest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task ThenSucceeded()
    {
        // Arrange
        var migrateRoadNetworkSqsRequest = ObjectProvider.Create<MigrateRoadNetworkSqsRequest>();
        var ticketId = ObjectProvider.Create<TicketId>();

        DataValidationApiClientMock
            .Setup(x => x.RequestDataValidationAsync(It.IsAny<CancellationToken>()))
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
        await HandleRequest(migrateRoadNetworkSqsRequest, ticketId: ticketId);

        // Assert
        var dataValidationQueueItem = ExtractsDbContext.DataValidationQueue.Single(x => x.UploadId == migrateRoadNetworkSqsRequest.UploadId.ToGuid());
        dataValidationQueueItem.SqsRequestJson.Should().NotBeNull();
        dataValidationQueueItem.DataValidationId.Should().NotBeNull();

        DataValidationApiClientMock.Verify(x =>
            x.RequestDataValidationAsync(It.IsAny<CancellationToken>()));
    }
}
