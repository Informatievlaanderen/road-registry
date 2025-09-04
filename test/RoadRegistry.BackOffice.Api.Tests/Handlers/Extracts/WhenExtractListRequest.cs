namespace RoadRegistry.BackOffice.Api.Tests.Handlers.Extracts;

using System.Linq;
using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using NodaTime;
using NodaTime.Testing;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Api.Handlers.Extracts;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice.Scenarios;

public class WhenExtractListRequest
{
    private readonly DbContextBuilder _dbContextBuilder;
    private readonly IClock _clock;
    private readonly Fixture _fixture;

    public WhenExtractListRequest(DbContextBuilder dbContextBuilder)
    {
        _dbContextBuilder = dbContextBuilder;
        _clock = new FakeClock(NodaConstants.UnixEpoch);
        _fixture = new RoadNetworkTestData().ObjectProvider;
    }

    [Fact]
    public async Task WithValidRequest_ThenResponse()
    {
        var request = new ExtractListRequest(null, 0);

        var extractRequestId = _fixture.Create<ExtractRequestId>();
        var downloadId = _fixture.Create<DownloadId>();

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractRequests.Add(new ExtractRequest
        {
            ExtractRequestId = extractRequestId,
            Description = _fixture.Create<string>(),
            CurrentDownloadId = downloadId,
            RequestedOn = _clock.GetCurrentInstant().ToDateTimeOffset()
        });
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = extractRequestId
        });
        await extractsDbContext.SaveChangesAsync();

        var handler = BuildHandler(extractsDbContext);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Items.Count.Should().Be(1);
        response.MoreDataAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task WithOrganizationCodeFilter_ThenResponse()
    {
        var organizationCode = _fixture.Create<string>();
        var request = new ExtractListRequest(organizationCode, 0);

        var extractRequestId1 = _fixture.Create<ExtractRequestId>();
        var extractRequestId2 = _fixture.Create<ExtractRequestId>();
        var downloadId1 = _fixture.Create<DownloadId>();
        var downloadId2 = _fixture.Create<DownloadId>();

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();

        {
            extractsDbContext.ExtractRequests.Add(new ExtractRequest
            {
                ExtractRequestId = extractRequestId1,
                Description = _fixture.Create<string>(),
                CurrentDownloadId = downloadId1,
                RequestedOn = _clock.GetCurrentInstant().ToDateTimeOffset(),
                OrganizationCode = organizationCode
            });
            extractsDbContext.ExtractDownloads.Add(new ExtractDownload
            {
                DownloadId = downloadId1,
                Contour = Polygon.Empty,
                ExtractRequestId = extractRequestId1
            });
        }
        {
            extractsDbContext.ExtractRequests.Add(new ExtractRequest
            {
                ExtractRequestId = extractRequestId2,
                Description = _fixture.Create<string>(),
                CurrentDownloadId = downloadId2,
                RequestedOn = _clock.GetCurrentInstant().ToDateTimeOffset(),
                OrganizationCode = _fixture.Create<string>()
            });
            extractsDbContext.ExtractDownloads.Add(new ExtractDownload
            {
                DownloadId = downloadId2,
                Contour = Polygon.Empty,
                ExtractRequestId = extractRequestId2
            });
        }

        await extractsDbContext.SaveChangesAsync();

        var handler = BuildHandler(extractsDbContext);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Items.Count.Should().Be(1);
        response.MoreDataAvailable.Should().BeFalse();
    }

    [Fact]
    public async Task WhenTheresMoreExtracts_ThenMoreDataAvailable()
    {
        var request = new ExtractListRequest(null, 0, 1);

        var extractRequestId1 = _fixture.Create<ExtractRequestId>();
        var extractRequestId2 = _fixture.Create<ExtractRequestId>();
        var downloadId1 = _fixture.Create<DownloadId>();
        var downloadId2 = _fixture.Create<DownloadId>();

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();

        {
            extractsDbContext.ExtractRequests.Add(new ExtractRequest
            {
                ExtractRequestId = extractRequestId1,
                Description = _fixture.Create<string>(),
                CurrentDownloadId = downloadId1,
                RequestedOn = _clock.GetCurrentInstant().ToDateTimeOffset()
            });
            extractsDbContext.ExtractDownloads.Add(new ExtractDownload
            {
                DownloadId = downloadId1,
                Contour = Polygon.Empty,
                ExtractRequestId = extractRequestId1
            });
        }
        {
            extractsDbContext.ExtractRequests.Add(new ExtractRequest
            {
                ExtractRequestId = extractRequestId2,
                Description = _fixture.Create<string>(),
                CurrentDownloadId = downloadId2,
                RequestedOn = _clock.GetCurrentInstant().ToDateTimeOffset()
            });
            extractsDbContext.ExtractDownloads.Add(new ExtractDownload
            {
                DownloadId = downloadId2,
                Contour = Polygon.Empty,
                ExtractRequestId = extractRequestId2
            });
        }

        await extractsDbContext.SaveChangesAsync();

        var handler = BuildHandler(extractsDbContext);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Items.Count.Should().Be(1);
        response.MoreDataAvailable.Should().BeTrue();
    }

    [Fact]
    public async Task WithPageIndexNotZero_ThenRetrieveRequestedPage()
    {
        var request = new ExtractListRequest(null, 1, 1);

        var extractRequestId1 = _fixture.Create<ExtractRequestId>();
        var extractRequestId2 = _fixture.Create<ExtractRequestId>();
        var downloadId1 = _fixture.Create<DownloadId>();
        var downloadId2 = _fixture.Create<DownloadId>();

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();

        {
            extractsDbContext.ExtractRequests.Add(new ExtractRequest
            {
                ExtractRequestId = extractRequestId1,
                Description = _fixture.Create<string>(),
                CurrentDownloadId = downloadId1,
                RequestedOn = _clock.GetCurrentInstant().ToDateTimeOffset()
            });
            extractsDbContext.ExtractDownloads.Add(new ExtractDownload
            {
                DownloadId = downloadId1,
                Contour = Polygon.Empty,
                ExtractRequestId = extractRequestId1
            });
        }
        {
            extractsDbContext.ExtractRequests.Add(new ExtractRequest
            {
                ExtractRequestId = extractRequestId2,
                Description = _fixture.Create<string>(),
                CurrentDownloadId = downloadId2,
                RequestedOn = _clock.GetCurrentInstant().ToDateTimeOffset()
            });
            extractsDbContext.ExtractDownloads.Add(new ExtractDownload
            {
                DownloadId = downloadId2,
                Contour = Polygon.Empty,
                ExtractRequestId = extractRequestId2
            });
        }

        await extractsDbContext.SaveChangesAsync();

        var handler = BuildHandler(extractsDbContext);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.Items.Count.Should().Be(1);
        response.Items.Single().DownloadId.Should().Be(downloadId2);
        response.MoreDataAvailable.Should().BeFalse();
    }

    private ExtractListRequestHandler BuildHandler(ExtractsDbContext dbContext)
    {
        return new ExtractListRequestHandler(
            dbContext,
            new FakeCommandHandlerDispatcher().Dispatcher,
            _clock,
            new NullLogger<ExtractListRequestHandler>());
    }
}
