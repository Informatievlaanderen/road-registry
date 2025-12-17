namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenCloseExtract;

using AutoFixture;
using FluentAssertions;
using Sqs.Extracts;
using Xunit.Abstractions;
using Polygon = NetTopologySuite.Geometries.Polygon;

public class WithValidRequest : WhenCloseExtractTestBase
{
    public WithValidRequest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task ThenSucceeded()
    {
        // Arrange
        var downloadId = ObjectProvider.Create<DownloadId>();
        var request = new CloseExtractSqsRequest
        {
            DownloadId = downloadId
        };

        ExtractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = ObjectProvider.Create<ExtractRequestId>()
        });
        await ExtractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(request);

        // Assert
        VerifyThatTicketHasCompleted(new object());

        var extractDownload = ExtractsDbContext.ExtractDownloads.Single(x => x.DownloadId == downloadId);
        extractDownload.Closed.Should().BeTrue();
    }
}
