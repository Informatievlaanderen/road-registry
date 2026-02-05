namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.Tests.Extracts.WhenUploadInwinningExtract;

using AutoFixture;
using Exceptions;
using FluentAssertions;
using Moq;
using Sqs.Extracts;
using TicketingService.Abstractions;
using Xunit.Abstractions;
using Polygon = NetTopologySuite.Geometries.Polygon;

public class WithInvalidRequest : WhenUploadInwinningExtractTestBase
{
    public WithInvalidRequest(ITestOutputHelper outputHelper) : base(outputHelper)
    {
    }

    [Fact]
    public async Task GivenCompletedInwinningszone_ThenException()
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
            Completed = true
        });

        await ExtractsDbContext.SaveChangesAsync();

        // Act
        await HandleRequest(request);

        // Assert
        TicketingMock.Verify(x =>
            x.Error(
                request.TicketId,
                It.Is<TicketError>(e => e.ErrorCode == "InwinningszoneIsGesloten"),
                CancellationToken.None
            )
        );
    }
}
