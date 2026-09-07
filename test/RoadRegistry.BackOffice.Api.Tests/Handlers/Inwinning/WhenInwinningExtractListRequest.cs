namespace RoadRegistry.BackOffice.Api.Tests.Handlers.Inwinning;

using System.Linq;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using NodaTime;
using NodaTime.Testing;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Api.Handlers.Inwinning;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice.Scenarios;

// A delivery that needs attention belongs at the top of the list, and one whose processing failed needs attention
// just as much as one that failed validation - it is the reason Digitaal Vlaanderen is mailed about it.
public class WhenInwinningExtractListRequest
{
    private readonly DbContextBuilder _dbContextBuilder;
    private readonly IClock _clock;
    private readonly Fixture _fixture;

    public WhenInwinningExtractListRequest(DbContextBuilder dbContextBuilder)
    {
        _dbContextBuilder = dbContextBuilder;
        _clock = new FakeClock(NodaConstants.UnixEpoch);
        _fixture = new RoadNetworkTestData().ObjectProvider;
    }

    [Theory]
    [InlineData(ExtractUploadStatus.ProcessingFailed)]
    [InlineData(ExtractUploadStatus.AutomaticValidationFailed)]
    [InlineData(ExtractUploadStatus.ManualValidationFailed)]
    public async Task ADeliveryThatNeedsAttentionIsListedFirst(ExtractUploadStatus status)
    {
        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();

        // The one that needs attention is requested first, so ordering by request date alone would put it last.
        var needsAttention = AddInwinningExtract(extractsDbContext, _clock.GetCurrentInstant().ToDateTimeOffset(), status);
        AddInwinningExtract(extractsDbContext, _clock.GetCurrentInstant().ToDateTimeOffset().AddDays(1), ExtractUploadStatus.Accepted);
        await extractsDbContext.SaveChangesAsync();

        var response = await BuildHandler(extractsDbContext).Handle(new InwinningExtractListRequest(null), CancellationToken.None);

        response.Items.Should().HaveCount(2);
        response.Items.First().DownloadId.Should().Be(needsAttention);
        response.Items.First().UploadStatus.Should().Be(status.ToString());
    }

    private DownloadId AddInwinningExtract(ExtractsDbContext extractsDbContext, DateTimeOffset requestedOn, ExtractUploadStatus uploadStatus)
    {
        var extractRequestId = _fixture.Create<ExtractRequestId>();
        var downloadId = _fixture.Create<DownloadId>();
        var uploadId = Guid.NewGuid();

        extractsDbContext.ExtractRequests.Add(new ExtractRequest
        {
            ExtractRequestId = extractRequestId,
            Description = _fixture.Create<string>(),
            CurrentDownloadId = downloadId,
            // The list is scoped to the inwinning deliveries by this prefix.
            ExternalRequestId = $"INWINNING_{_fixture.Create<string>()}"
        });
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = extractRequestId,
            RequestedOn = requestedOn,
            LatestUploadId = uploadId
        });
        extractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = uploadId,
            DownloadId = downloadId,
            UploadedOn = requestedOn,
            Status = uploadStatus,
            TicketId = Guid.NewGuid()
        });

        return downloadId;
    }

    private InwinningExtractListRequestHandler BuildHandler(ExtractsDbContext dbContext)
    {
        return new InwinningExtractListRequestHandler(
            dbContext,
            new FakeCommandHandlerDispatcher().Dispatcher,
            new NullLogger<InwinningExtractListRequestHandler>());
    }
}
