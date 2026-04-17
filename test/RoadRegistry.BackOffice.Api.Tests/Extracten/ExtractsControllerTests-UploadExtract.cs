namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using System.Linq;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.BackOffice.Abstractions.Jobs;
using RoadRegistry.BackOffice.Api.Extracten;
using RoadRegistry.Extracts.Schema;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task WhenUploadingExtract_ThenSucceeded()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();

        var presignedUrl = Fixture.Create<string>();
        Mediator
            .Setup(x => x.Send(It.IsAny<GetPresignedUploadUrlRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPresignedUploadUrlResponse(Guid.NewGuid(), presignedUrl, [], Guid.NewGuid(), string.Empty));

        _extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = Fixture.Create<ExtractRequestId>()
        });
        await _extractsDbContext.SaveChangesAsync();

        // Act
        var result = await Controller.UploadExtract(
            downloadId,
            _extractsDbContext);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ExtractenController.UploadExtractResponse>(okObjectResult.Value);
        response.UploadUrl.Should().Be(presignedUrl);

        var extractDownload = _extractsDbContext.ExtractDownloads.Single();
        extractDownload.TicketId.Should().NotBeNull();
    }

    [Fact]
    public async Task WhenUploadingExtract_WithInvalidDownloadId_ThenValidationException()
    {
        var act = () => Controller.UploadExtract(
            "not_a_guid_without_dashes",
            _extractsDbContext);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task WhenUploadingExtract_WithUnknownDownloadId_ThenNotFound()
    {
        var downloadId = Fixture.Create<DownloadId>();
        var response = await Controller.UploadExtract(
            downloadId,
            _extractsDbContext);

        Assert.IsType<NotFoundResult>(response);
    }

    [Fact]
    public async Task WhenExtractClosed_WThenValidationException()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();

        var presignedUrl = Fixture.Create<string>();
        Mediator
            .Setup(x => x.Send(It.IsAny<GetPresignedUploadUrlRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPresignedUploadUrlResponse(Guid.NewGuid(), presignedUrl, [], Guid.NewGuid(), string.Empty));

        _extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = Fixture.Create<ExtractRequestId>(),
            TicketId = Guid.NewGuid(),
            Closed = true
        });
        await _extractsDbContext.SaveChangesAsync();

        // Act
        var act = () => Controller.UploadExtract(
            downloadId,
            _extractsDbContext);

        // Assert
        var assertion = await act.Should().ThrowAsync<ValidationException>();
        var ex = assertion.Which;
        ex.Errors.Count().Should().Be(1);
        ex.Errors.First().ErrorCode.Should().Be("ExtractGesloten");
    }

    [Theory]
    [InlineData(ExtractUploadStatus.Processing)]
    [InlineData(ExtractUploadStatus.AutomaticValidationSucceeded)]
    public async Task WhenUploadingExtract_WithPreviousUploadNotCompleted_ThenValidationException(ExtractUploadStatus incompleteUploadStatus)
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();

        var presignedUrl = Fixture.Create<string>();
        Mediator
            .Setup(x => x.Send(It.IsAny<GetPresignedUploadUrlRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPresignedUploadUrlResponse(Guid.NewGuid(), presignedUrl, [], Guid.NewGuid(), string.Empty));

        var uploadId = Fixture.Create<UploadId>();
        _extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = Fixture.Create<ExtractRequestId>(),
            TicketId = Guid.NewGuid(),
            LatestUploadId = uploadId
        });
        _extractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = uploadId,
            DownloadId = downloadId,
            Status = incompleteUploadStatus,
            UploadedOn = DateTimeOffset.UtcNow,
            TicketId = Guid.NewGuid()
        });
        await _extractsDbContext.SaveChangesAsync();

        // Act
        var act = () => Controller.UploadExtract(
            downloadId,
            _extractsDbContext);

        // Assert
        var assertion = await act.Should().ThrowAsync<ValidationException>();
        var ex = assertion.Which;
        ex.Errors.Count().Should().Be(1);
        ex.Errors.First().ErrorCode.Should().Be("UploadNietAfgerond");
    }

    [Theory]
    [InlineData(ExtractUploadStatus.AutomaticValidationFailed)]
    [InlineData(ExtractUploadStatus.ManualValidationFailed)]
    public async Task WhenUploadingExtract_WithPreviousUploadFailed_ThenSucceeded(ExtractUploadStatus uploadFailedStatus)
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();

        var presignedUrl = Fixture.Create<string>();
        Mediator
            .Setup(x => x.Send(It.IsAny<GetPresignedUploadUrlRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new GetPresignedUploadUrlResponse(Guid.NewGuid(), presignedUrl, [], Guid.NewGuid(), string.Empty));

        var uploadId = Fixture.Create<UploadId>();
        _extractsDbContext.ExtractDownloads.Add(new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = Fixture.Create<ExtractRequestId>(),
            TicketId = Guid.NewGuid(),
            LatestUploadId = uploadId
        });
        _extractsDbContext.ExtractUploads.Add(new ExtractUpload
        {
            UploadId = uploadId,
            DownloadId = downloadId,
            Status = uploadFailedStatus,
            UploadedOn = DateTimeOffset.UtcNow,
            TicketId = Guid.NewGuid()
        });
        await _extractsDbContext.SaveChangesAsync();

        // Act
        var result = await Controller.UploadExtract(
            downloadId,
            _extractsDbContext);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ExtractenController.UploadExtractResponse>(okObjectResult.Value);
        response.UploadUrl.Should().Be(presignedUrl);
    }
}
