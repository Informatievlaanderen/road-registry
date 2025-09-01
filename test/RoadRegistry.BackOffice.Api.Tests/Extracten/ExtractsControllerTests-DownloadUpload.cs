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
    public async Task WhenDownloadingUpload_ThenOkResult()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();

        var presignedUrl = Fixture.Create<string>();
        var fileName = Fixture.Create<string>();
        Mediator
            .Setup(x => x.Send(It.IsAny<GetDownloadUploadPresignedUrlRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetDownloadUploadPresignedUrlResponse(presignedUrl, fileName));

        // Act
        var result = await Controller.DownloadUpload(
            downloadId);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ExtractenController.DownloadUploadResponse>(okObjectResult.Value);
        response.DownloadUrl.Should().Be(presignedUrl);
        response.FileName.Should().Be(fileName);
    }

    [Fact]
    public async Task WhenDownloadingUpload_WithInvalidDownloadId_ThenValidationException()
    {
        try
        {
            await Controller.DownloadUpload(
                "not_a_guid_without_dashes");
            Assert.Fail("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }

    [Fact]
    public async Task WhenDownloadingUpload_WithUnknownDownloadId_ThenNotFound()
    {
        var downloadId = Fixture.Create<DownloadId>();

        Mediator
            .Setup(x => x.Send(It.IsAny<GetDownloadUploadPresignedUrlRequest>(), It.IsAny<CancellationToken>()))
            .Throws(new ExtractDownloadNotFoundException(downloadId));

        var result = await Controller.DownloadUpload(
            downloadId);

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task WhenDownloadingUpload_WithRemovedBlob_ThenGone()
    {
        var downloadId = Fixture.Create<DownloadId>();

        Mediator
            .Setup(x => x.Send(It.IsAny<GetDownloadUploadPresignedUrlRequest>(), It.IsAny<CancellationToken>()))
            .Throws(new BlobNotFoundException(new BlobName(Fixture.Create<string>())));

        var result = await Controller.DownloadUpload(
            downloadId);

        var statusCodeResult = Assert.IsType<StatusCodeResult>(result);
        statusCodeResult.StatusCode.Should().Be(410);
    }
}
