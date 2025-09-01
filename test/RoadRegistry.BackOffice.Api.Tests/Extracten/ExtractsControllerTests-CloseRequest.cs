namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using AutoFixture;
using BackOffice.Handlers.Sqs.Extracts;
using Be.Vlaanderen.Basisregisters.Sqs.Requests;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite.Geometries;
using RoadRegistry.Extracts.Schema;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task WhenClosingExtract_ThenAcceptedResult()
    {
        // Arrange
        var downloadId = Fixture.Create<DownloadId>();
        var extract = new ExtractDownload
        {
            DownloadId = downloadId,
            Contour = Polygon.Empty,
            ExtractRequestId = Fixture.Create<ExtractRequestId>()
        };
        _extractsDbContext.ExtractDownloads.Add(extract);
        await _extractsDbContext.SaveChangesAsync();

        var locationResult = Fixture.Create<LocationResult>();
        Mediator
            .Setup(x => x.Send(It.IsAny<CloseExtractSqsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(locationResult);

        // Act
        var result = await Controller.SluitExtract(
            downloadId,
            _extractsDbContext);

        // Assert
        var acceptedResult = Assert.IsType<AcceptedResult>(result);
        acceptedResult.Location.Should().Be(locationResult.Location.ToString());
    }

    [Fact]
    public async Task WhenClosingExtract_WithInvalidDownloadId_ThenValidationException()
    {
        try
        {
            await Controller.SluitExtract(
                "not_a_guid_without_dashes",
                _extractsDbContext);
            Assert.Fail("Expected a validation exception but did not receive any");
        }
        catch (ValidationException)
        {
        }
    }

    [Fact]
    public async Task WhenClosingExtract_WithUnknownDownloadId_ThenNotFound()
    {
        var downloadId = Fixture.Create<DownloadId>();
        var response= await Controller.SluitExtract(
            downloadId,
            _extractsDbContext);

        Assert.IsType<NotFoundResult>(response);
    }
}
