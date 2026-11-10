namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using Abstractions.Extracts.V2;
using Api.Extracten;
using AutoFixture;
using Exceptions;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Extensions;
using RoadRegistry.Extensions;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task WhenGettingExtractDetails_ThenSucceeded()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();

        var extractDetailsResponse = new ExtractDetailsResponse
        {
            DownloadId = downloadId,
            Contour = MultiPolygon.Empty
        };

        Mediator
            .Setup(x => x.Send(It.IsAny<ExtractDetailsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(extractDetailsResponse);

        // Act
        var result = await Controller.GetExtractDetails(
            downloadId,
            CancellationToken.None);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<ExtractDetailsResponseBody>(okObjectResult.Value);

        responseObject.DownloadId.Should().Be(extractDetailsResponse.DownloadId.ToString());
    }

    [Fact]
    public async Task WhenGettingExtractDetails_WithInvalidDownloadId_ThenValidationException()
    {
        var act = () => Controller.GetExtractDetails(
            "not_a_guid_without_dashes",
            CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task WhenGettingExtractDetails_WithUnknownDownloadId_ThenNotFound()
    {
        var downloadId = Fixture.Create<DownloadId>();

        Mediator
            .Setup(x => x.Send(It.IsAny<ExtractDetailsRequest>(), It.IsAny<CancellationToken>()))
            .Throws(new ExtractRequestNotFoundException(downloadId));

        var result = await Controller.GetExtractDetails(
            downloadId,
            CancellationToken.None);

        result.Should().BeOfType<NotFoundResult>();
    }

    // Contours are stored in Lambert 2008. The v1 endpoints work in Lambert 72, so their detail puts the contour back
    // into Lambert 72 and the contract stays what it was; the v2 endpoints hand it back in Lambert 2008.
    [Fact]
    public async Task WhenGettingExtractDetailsThroughV1_ThenTheContourIsInLambert72()
    {
        var contour = GivenExtractDetailsWithContourInLambert08();

        var result = await Controller.GetExtractDetails(Fixture.Create<DownloadId>(), CancellationToken.None);

        var responseObject = Assert.IsType<ExtractDetailsResponseBody>(Assert.IsType<OkObjectResult>(result).Value);
        responseObject.Contour.Should().Be(contour.ToExtractGeometry().EnsureLambert72().Value.ToGeoJson());
        responseObject.Contour.Should().NotBe(contour.ToGeoJson());
    }

    [Fact]
    public async Task WhenGettingExtractDetailsThroughV2_ThenTheContourIsInLambert08()
    {
        var contour = GivenExtractDetailsWithContourInLambert08();

        var result = await Controller.GetExtractDetailsV2(Fixture.Create<DownloadId>(), CancellationToken.None);

        var responseObject = Assert.IsType<ExtractDetailsResponseBody>(Assert.IsType<OkObjectResult>(result).Value);
        responseObject.Contour.Should().Be(contour.ToGeoJson());
    }

    private MultiPolygon GivenExtractDetailsWithContourInLambert08()
    {
        var contour = (MultiPolygon)new NetTopologySuite.IO.WKTReader().Read("MULTIPOLYGON (((655000 710000, 655000 711000, 656000 711000, 656000 710000, 655000 710000)))");
        contour.SRID = WellknownSrids.Lambert08;

        Mediator
            .Setup(x => x.Send(It.IsAny<ExtractDetailsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExtractDetailsResponse
            {
                DownloadId = Fixture.Create<DownloadId>(),
                Contour = contour
            });

        return contour;
    }
}
