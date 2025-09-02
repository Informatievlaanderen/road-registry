namespace RoadRegistry.Jobs.Processor.Tests;

using Editor.Schema;
using Extracts.Schema;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Point = NetTopologySuite.Geometries.Point;

public partial class ExtractRequestCleanerTests
{
    [Fact]
    public async Task ExtractsV2_GivenDownloadedExtracts_ThenCloseExtractsOlderThan6Months()
    {
        // Arrange
        var commandHandlerDispatcher = new FakeCommandHandlerDispatcher();
        await using var editorContext = new FakeEditorContextFactory().CreateDbContext();
        await using var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();

        var oldExtractDownloadId = Guid.NewGuid();
        extractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = oldExtractDownloadId,
            ExtractRequestId = "1",
            RequestedOn = DateTimeOffset.Now.AddMonths(-7),
            DownloadedOn = DateTimeOffset.Now,
            IsInformative = false,
            Contour = new Point(0, 0)
        });
        extractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = Guid.NewGuid(),
            ExtractRequestId = "2",
            RequestedOn = DateTimeOffset.Now.AddMonths(-1),
            DownloadedOn = DateTimeOffset.Now,
            IsInformative = false,
            Contour = new Point(0, 0)
        });
        await extractsDbContext.SaveChangesAsync();

        var cleaner = new ExtractRequestCleaner(
            commandHandlerDispatcher.Dispatcher,
            editorContext,
            extractsDbContext,
            new NullLoggerFactory()
        );

        // Act
        await cleaner.CloseOldExtracts(CancellationToken.None);

        // Assert
        var oldExtractDownload = extractsDbContext.ExtractDownloads.Single(x => x.DownloadId == oldExtractDownloadId);
        oldExtractDownload.Closed.Should().BeTrue();
    }

    [Fact]
    public async Task ExtractsV2_GivenNotDownloadedExtracts_ThenCloseExtractsOlderThanAWeek()
    {
        // Arrange
        var commandHandlerDispatcher = new FakeCommandHandlerDispatcher();
        await using var editorContext = new FakeEditorContextFactory().CreateDbContext();
        await using var extractsDbContext = new FakeExtractsDbContextFactory().CreateDbContext();

        var oldExtractDownloadId = Guid.NewGuid();
        extractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = oldExtractDownloadId,
            ExtractRequestId = "1",
            RequestedOn = DateTimeOffset.Now.AddDays(-8),
            DownloadedOn = null,
            IsInformative = false,
            Contour = new Point(0, 0)
        });
        extractsDbContext.ExtractDownloads.Add(new()
        {
            DownloadId = Guid.NewGuid(),
            ExtractRequestId = "2",
            RequestedOn = DateTimeOffset.Now.AddMonths(-1),
            DownloadedOn = DateTimeOffset.Now,
            IsInformative = false,
            Contour = new Point(0, 0)
        });
        await extractsDbContext.SaveChangesAsync();

        var cleaner = new ExtractRequestCleaner(
            commandHandlerDispatcher.Dispatcher,
            editorContext,
            extractsDbContext,
            new NullLoggerFactory()
        );

        // Act
        await cleaner.CloseOldExtracts(CancellationToken.None);

        // Assert
        var oldExtractDownload = extractsDbContext.ExtractDownloads.Single(x => x.DownloadId == oldExtractDownloadId);
        oldExtractDownload.Closed.Should().BeTrue();
    }
}
