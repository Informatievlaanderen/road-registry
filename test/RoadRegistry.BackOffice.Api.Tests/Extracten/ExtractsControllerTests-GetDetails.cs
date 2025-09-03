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
}
