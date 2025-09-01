namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using System.Collections.Generic;
using System.Linq;
using Abstractions.Jobs;
using Api.Extracten;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.Schema;
using TicketingService.Abstractions;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task WhenUploadingExtract_ThenOkResult()
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
            _extractsDbContext,
            Mock.Of<ITicketing>());

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
        try
        {
            await Controller.UploadExtract(
                "not_a_guid_without_dashes",
                _extractsDbContext,
                Mock.Of<ITicketing>());
            Assert.Fail("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }

    [Fact]
    public async Task WhenUploadingExtract_WithUnknownDownloadId_ThenNotFound()
    {
        var downloadId = Fixture.Create<DownloadId>();
        var response = await Controller.UploadExtract(
            downloadId,
            _extractsDbContext,
            Mock.Of<ITicketing>());

        Assert.IsType<NotFoundResult>(response);
    }

    [Theory]
    [InlineData(TicketStatus.Created)]
    [InlineData(TicketStatus.Pending)]
    public async Task WhenUploadingExtract_WithPreviousUploadNotCompleted_ThenValidationException(TicketStatus incompleteTicketStatus)
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
            TicketId = Guid.NewGuid()
        });
        await _extractsDbContext.SaveChangesAsync();

        var ticketing = new Mock<ITicketing>();
        ticketing
            .Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ticket(Guid.NewGuid(), incompleteTicketStatus, new Dictionary<string, string>()));

        // Act
        var act = () => Controller.UploadExtract(
            downloadId,
            _extractsDbContext,
            ticketing.Object);

        // Assert
        var assertion = await act.Should().ThrowAsync<ValidationException>();
        var ex = assertion.Which;
        ex.Errors.Count().Should().Be(1);
        ex.Errors.First().ErrorCode.Should().Be("UploadNietAfgerond");
    }

    [Theory]
    [InlineData(TicketStatus.Complete)]
    [InlineData(TicketStatus.Error)]
    public async Task WhenUploadingExtract_WithPreviousUploadCompleted_ThenOkResult(TicketStatus completedTicketStatus)
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
            TicketId = Guid.NewGuid()
        });
        await _extractsDbContext.SaveChangesAsync();

        var ticketing = new Mock<ITicketing>();
        ticketing
            .Setup(x => x.Get(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Ticket(Guid.NewGuid(), completedTicketStatus, new Dictionary<string, string>()));

        // Act
        var result = await Controller.UploadExtract(
            downloadId,
            _extractsDbContext,
            ticketing.Object);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ExtractenController.UploadExtractResponse>(okObjectResult.Value);
        response.UploadUrl.Should().Be(presignedUrl);
    }
}
