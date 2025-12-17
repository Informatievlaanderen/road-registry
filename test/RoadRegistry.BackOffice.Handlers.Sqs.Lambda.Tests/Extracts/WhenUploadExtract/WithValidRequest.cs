namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenUploadExtract;

using AutoFixture;
using BackOffice.Framework;
using Be.Vlaanderen.Basisregisters.BlobStore;
using FluentAssertions;
using Messages;
using Moq;
using Newtonsoft.Json;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice;
using SqlStreamStore.Streams;
using Sqs.Extracts;
using TicketingService.Abstractions;
using Xunit.Abstractions;
using Polygon = NetTopologySuite.Geometries.Polygon;

public class WithValidRequest : WhenUploadExtractTestBase
{
    public WithValidRequest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task ThenSucceeded()
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

        // Act
        var sqsRequest = await HandleRequest(request);

        // Assert
        TicketingMock.Verify(x =>
            x.Pending(
                sqsRequest.TicketId,
                CancellationToken.None
            ), Times.Never
        );
        TicketingMock.Verify(x =>
            x.Complete(
                sqsRequest.TicketId,
                It.IsAny<TicketResult>(),
                CancellationToken.None
            ), Times.Never
        );

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.UploadId.Should().Be(request.UploadId);
        extractDownload.UploadedOn.Should().NotBeNull();
        extractDownload.UploadStatus.Should().Be(ExtractUploadStatus.Processing);

        var lastMessage = (await StreamStore.ReadAllBackwards(Position.End, 1)).Messages.Single();
        var messageMetadata = lastMessage.JsonMetadataAs<MessageMetadata>();
        messageMetadata.Processor.Should().Be(RoadRegistryApplication.BackOffice);

        var changeRoadNetworkCommand = JsonConvert.DeserializeObject<ChangeRoadNetwork>(await lastMessage.GetJsonData());
        changeRoadNetworkCommand.Should().NotBeNull();
        changeRoadNetworkCommand!.ExtractRequestId.Should().Be(request.ExtractRequestId);
        changeRoadNetworkCommand.DownloadId.Should().Be(request.DownloadId);
        changeRoadNetworkCommand.TicketId.Should().Be(sqsRequest.TicketId);
        changeRoadNetworkCommand.UseExtractsV2.Should().BeTrue();
    }
}
