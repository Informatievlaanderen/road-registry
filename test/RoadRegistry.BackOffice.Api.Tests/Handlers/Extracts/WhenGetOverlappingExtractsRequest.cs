namespace RoadRegistry.BackOffice.Api.Tests.Handlers.Extracts;

using System.Linq;
using Abstractions.Extracts;
using AutoFixture;
using BackOffice.Handlers.Extracts;
using Be.Vlaanderen.Basisregisters.Shaperon.Geometries;
using FluentAssertions;
using Infrastructure;
using Microsoft.Extensions.Logging.Abstractions;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using RoadRegistry.Extracts.Schema;
using RoadRegistry.Tests.BackOffice.Scenarios;
using GeometryTranslator = BackOffice.GeometryTranslator;

public class WhenGetOverlappingExtractsRequest
{
    private readonly DbContextBuilder _dbContextBuilder;
    private readonly Fixture _fixture;

    public WhenGetOverlappingExtractsRequest(DbContextBuilder dbContextBuilder)
    {
        _dbContextBuilder = dbContextBuilder;
        _fixture = new RoadNetworkTestData().ObjectProvider;
    }

    [Fact]
    public async Task GivenExtracts_WithValidRequest_ThenResponse()
    {
        var contour = BuildContour("MULTIPOLYGON (((0 0, 0 10, 20 10, 20 0, 0 0)))");
        var notOverlappingContour = BuildContour("MULTIPOLYGON (((100 0, 100 10, 120 10, 120 0, 100 0)))");
        var overlappingContour = BuildContour("MULTIPOLYGON (((5 0, 5 10, 25 10, 25 0, 5 0)))");
        var contourDownloadId = Guid.NewGuid();
        var overlappingContourDownloadId = Guid.NewGuid();

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = contourDownloadId,
            Contour = contour.WithSrid(GeometryConfiguration.GeometryFactory.SRID),
            ExtractRequestId = _fixture.Create<ExtractRequestId>()
        });
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = overlappingContourDownloadId,
            Contour = overlappingContour,
            ExtractRequestId = _fixture.Create<ExtractRequestId>()
        });
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = Guid.NewGuid(),
            Contour = notOverlappingContour,
            ExtractRequestId = _fixture.Create<ExtractRequestId>()
        });
        await extractsDbContext.SaveChangesAsync();

        var handler = BuildHandler(extractsDbContext);
        var request = new GetOverlappingExtractsRequest(overlappingContour);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.DownloadIds.Should().HaveCount(2);
        response.DownloadIds.Should().Contain(contourDownloadId);
        response.DownloadIds.Should().Contain(overlappingContourDownloadId);
    }

    [Fact]
    public async Task GivenMunicipalityExtracts_WithMunicipalityExtract_ThenResponse()
    {
        var contour = BuildContour("MULTIPOLYGON (((0 0, 0 10, 20 10, 20 0, 0 0)))");

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = Guid.NewGuid(),
            Contour = contour,
            ExtractRequestId = _fixture.Create<ExtractRequestId>()
        });
        await extractsDbContext.SaveChangesAsync();

        var handler = BuildHandler(extractsDbContext);
        var request = new GetOverlappingExtractsRequest(contour);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.DownloadIds.Should().HaveCount(1);
    }

    [Fact]
    public async Task GivenAdjacentMunicipalityExtracts_WithMunicipalityExtract_ThenOnlyMatchOnSelfNotAdjacent()
    {
        var contourLeft = BuildContour("MULTIPOLYGON (((0 0, 0 10, 20 10, 20 0, 0 0)))");
        var contourMiddle = BuildContour("MULTIPOLYGON (((20 0, 20 10, 40 10, 40 0, 20 0)))");
        var contourRight = BuildContour("MULTIPOLYGON (((40 0, 40 10, 60 10, 60 0, 40 0)))");
        var middleDownloadId = Guid.NewGuid();

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = Guid.NewGuid(),
            Contour = contourLeft,
            ExtractRequestId = _fixture.Create<ExtractRequestId>()
        });
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = middleDownloadId,
            Contour = contourMiddle,
            ExtractRequestId = _fixture.Create<ExtractRequestId>()
        });
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = Guid.NewGuid(),
            Contour = contourRight,
            ExtractRequestId = _fixture.Create<ExtractRequestId>()
        });
        await extractsDbContext.SaveChangesAsync();

        var handler = BuildHandler(extractsDbContext);
        var request = new GetOverlappingExtractsRequest(contourMiddle);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.DownloadIds.Should().HaveCount(1);
        response.DownloadIds.Single().Should().Be(middleDownloadId);
    }

    [Fact]
    public async Task GivenInformativeAndClosedExtracts_ThenExcludeInformativeAndClosed()
    {
        var contour = BuildContour("MULTIPOLYGON (((0 0, 0 10, 20 10, 20 0, 0 0)))");
        var openDownloadId = Guid.NewGuid();

        var extractsDbContext = _dbContextBuilder.CreateExtractsDbContext();
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = openDownloadId,
            Contour = contour,
            ExtractRequestId = _fixture.Create<ExtractRequestId>()
        });
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = Guid.NewGuid(),
            Contour = contour,
            ExtractRequestId = _fixture.Create<ExtractRequestId>(),
            IsInformative = true
        });
        extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = Guid.NewGuid(),
            Contour = contour,
            ExtractRequestId = _fixture.Create<ExtractRequestId>(),
            Closed = true
        });
        await extractsDbContext.SaveChangesAsync();

        var handler = BuildHandler(extractsDbContext);
        var request = new GetOverlappingExtractsRequest(contour);

        // Act
        var response = await handler.Handle(request, CancellationToken.None);

        // Assert
        response.Should().NotBeNull();
        response.DownloadIds.Should().HaveCount(1);
        response.DownloadIds.Single().Should().Be(openDownloadId);
    }

    private Geometry BuildContour(string wkt)
    {
        var contour = (IPolygonal)new WKTReader().Read(wkt);

        return (Geometry)GeometryTranslator.Translate(GeometryTranslator.TranslateToRoadNetworkExtractGeometry(contour));
    }

    private GetOverlappingExtractsRequestHandler BuildHandler(ExtractsDbContext dbContext)
    {
        return new GetOverlappingExtractsRequestHandler(
            dbContext,
            new FakeCommandHandlerDispatcher().Dispatcher,
            new NullLoggerFactory());
    }
}
