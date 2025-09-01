namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using Abstractions.Extracts.V2;
using Api.Extracten;
using AutoFixture;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Exceptions;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task WhenDownloadingExtract_ThenOkResult()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();

        var presignedUrl = Fixture.Create<string>();
        Mediator
            .Setup(x => x.Send(It.IsAny<GetDownloadExtractPresignedUrlRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetDownloadExtractPresignedUrlResponse(presignedUrl));

        // Act
        var result = await Controller.DownloadExtract(
            downloadId);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ExtractenController.DownloadExtractResponse>(okObjectResult.Value);
        response.DownloadUrl.Should().Be(presignedUrl);
    }

    [Fact]
    public async Task WhenDownloadingExtract_WithInvalidDownloadId_ThenValidationException()
    {
        try
        {
            await Controller.DownloadExtract(
                "not_a_guid_without_dashes");
            Assert.Fail("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }

    [Fact]
    public async Task WhenDownloadingExtract_WithUnknownDownloadId_ThenNotFound()
    {
        var downloadId = Fixture.Create<DownloadId>();

        Mediator
            .Setup(x => x.Send(It.IsAny<GetDownloadExtractPresignedUrlRequest>(), It.IsAny<CancellationToken>()))
            .Throws(new ExtractDownloadNotFoundException(downloadId));

        var result = await Controller.DownloadExtract(
            downloadId);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task WhenDownloadingExtract_WithRemovedBlob_ThenGone()
    {
        var downloadId = Fixture.Create<DownloadId>();

        Mediator
            .Setup(x => x.Send(It.IsAny<GetDownloadExtractPresignedUrlRequest>(), It.IsAny<CancellationToken>()))
            .Throws(new BlobNotFoundException(new BlobName(Fixture.Create<string>())));

        var result = await Controller.DownloadExtract(
            downloadId);

        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        statusCodeResult.StatusCode.Should().Be(410);
    }
}
