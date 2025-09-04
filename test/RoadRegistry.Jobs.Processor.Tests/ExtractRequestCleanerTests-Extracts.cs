namespace RoadRegistry.Jobs.Processor.Tests;

using BackOffice;
using BackOffice.Messages;
using Editor.Schema;
using Editor.Schema.Extracts;
using Extracts.Schema;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Point = NetTopologySuite.Geometries.Point;

public partial class ExtractRequestCleanerTests
{
    [Fact]
    public async Task ExtractsV1_GivenDownloadedExtracts_ThenCloseExtractsOlderThan6Months()
    {
        // Arrange
        var commandHandlerDispatcher = new FakeCommandHandlerDispatcher();
        await using var editorContext = new FakeEditorContextFactory().CreateDbContext();
        await using var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();

        var oldExtractDownloadId = Guid.NewGuid();
        editorContext.ExtractRequests.Add(new ExtractRequestRecord
        {
            DownloadId = oldExtractDownloadId,
            ExternalRequestId = "external-request-id-1",
            RequestedOn = DateTimeOffset.Now.AddMonths(-7),
            DownloadedOn = DateTimeOffset.Now,
            IsInformative = false,
            Contour = new Point(0, 0)
        });
        editorContext.ExtractRequests.Add(new ExtractRequestRecord
        {
            DownloadId = Guid.NewGuid(),
            ExternalRequestId = "external-request-id-2",
            RequestedOn = DateTimeOffset.Now.AddMonths(-1),
            DownloadedOn = DateTimeOffset.Now,
            IsInformative = false,
            Contour = new Point(0, 0)
        });
        await editorContext.SaveChangesAsync();

        var cleaner = new ExtractRequestCleaner(
            commandHandlerDispatcher.Dispatcher,
            editorContext,
            extractsDbContext,
            new NullLoggerFactory()
        );

        // Act
        await cleaner.CloseOldExtracts(CancellationToken.None);

        // Assert
        commandHandlerDispatcher.Invocations.Should().NotBeEmpty();
        var command = commandHandlerDispatcher.Invocations.Single().Body.Should().BeOfType<CloseRoadNetworkExtract>().Subject;
        command.DownloadId.Should().Be(new DownloadId(oldExtractDownloadId));
    }

    [Fact]
    public async Task ExtractsV1_GivenNotDownloadedExtracts_ThenCloseExtractsOlderThanAWeek()
    {
        // Arrange
        var commandHandlerDispatcher = new FakeCommandHandlerDispatcher();
        await using var editorContext = new FakeEditorContextFactory().CreateDbContext();
        await using var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();

        var oldExtractDownloadId = Guid.NewGuid();
        editorContext.ExtractRequests.Add(new ExtractRequestRecord
        {
            DownloadId = oldExtractDownloadId,
            ExternalRequestId = "external-request-id-1",
            RequestedOn = DateTimeOffset.Now.AddDays(-8),
            DownloadedOn = null,
            IsInformative = false,
            Contour = new Point(0, 0)
        });
        editorContext.ExtractRequests.Add(new ExtractRequestRecord
        {
            DownloadId = Guid.NewGuid(),
            ExternalRequestId = "external-request-id-2",
            RequestedOn = DateTimeOffset.Now.AddMonths(-1),
            DownloadedOn = DateTimeOffset.Now,
            IsInformative = false,
            Contour = new Point(0, 0)
        });
        await editorContext.SaveChangesAsync();

        var cleaner = new ExtractRequestCleaner(
            commandHandlerDispatcher.Dispatcher,
            editorContext,
            extractsDbContext,
            new NullLoggerFactory()
        );

        // Act
        await cleaner.CloseOldExtracts(CancellationToken.None);

        // Assert
        commandHandlerDispatcher.Invocations.Should().NotBeEmpty();
        var command = commandHandlerDispatcher.Invocations.Single().Body.Should().BeOfType<CloseRoadNetworkExtract>().Subject;
        command.DownloadId.Should().Be(new DownloadId(oldExtractDownloadId));
    }
}
