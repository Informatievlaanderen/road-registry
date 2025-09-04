namespace RoadRegistry.BackOffice.Api.Tests.Handlers.Extracts;

using AutoFixture;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Abstractions.Extracts.V2;
using RoadRegistry.BackOffice.Api.Handlers.Extracts;
using RoadRegistry.BackOffice.Api.Tests.Infrastructure;
using RoadRegistry.BackOffice.Exceptions;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice.Scenarios;

public class WhenExtractDetailsRequest
{
    private readonly DbContextBuilder _dbContextBuilder;
    private readonly Fixture _fixture;

    public WhenExtractDetailsRequest(DbContextBuilder dbContextBuilder)
    {
        _dbContextBuilder = dbContextBuilder;
        _fixture = new RoadNetworkTestData().ObjectProvider;
    }

    [Fact]
    public async Task WithValidRequest_ThenResponse()
    {
        var request = new ExtractDetailsRequest(_fixture.Create<DownloadId>());

        var extractRequestId = _fixture.Create<ExtractRequestId>();

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractRequests.Add(new ExtractRequest
        {
            ExtractRequestId = extractRequestId,
            Description = _fixture.Create<string>()
        });
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = request.DownloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = extractRequestId
        });
        await extractsDbContext.SaveChangesAsync();

        var handler = BuildHandler(extractsDbContext);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.DownloadId.Should().Be(request.DownloadId);
    }

    [Fact]
    public async Task WithUnknownDownloadId_ThenExtractRequestNotFoundException()
    {
        var request = new ExtractDetailsRequest(_fixture.Create<DownloadId>());

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        var handler = BuildHandler(extractsDbContext);

        // Act
        var act = () => handler.Handle(request, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<ExtractRequestNotFoundException>();
    }

    private ExtractDetailsRequestHandler BuildHandler(ExtractsDbContext dbContext)
    {
        return new ExtractDetailsRequestHandler(
            dbContext,
            new FakeCommandHandlerDispatcher().Dispatcher,
            new NullLogger<ExtractDetailsRequestHandler>());
    }
}
