namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenUploadInwinningExtract;

using AutoFixture;
using Moq;
using RoadNetwork;
using Sqs.Extracts;
using TicketingService.Abstractions;
using Xunit.Abstractions;
using Polygon = NetTopologySuite.Geometries.Polygon;

public class WithValidRequest : WhenUploadInwinningExtractTestBase
{
    public WithValidRequest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task ThenSucceeded()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new UploadInwinningExtractSqsRequest
        {
            DownloadId = downloadId,
            UploadId = ObjectProvider.Create<UploadId>(),
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        };

        ExtractsDbContext.Inwinningszones.Add(new()
        {
            NisCode = ObjectProvider.Create<string>(),
            Contour = Polygon.Empty,
            Operator = ObjectProvider.Create<string>(),
            DownloadId = downloadId,
            Completed = false
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

        MediatorMock
            .Verify(x => x.Send(
                It.Is<MigrateDryRunRoadNetworkSqsRequest>(r => r.DownloadId == downloadId),
                It.IsAny<CancellationToken>()), Times.Once);
    }
}
