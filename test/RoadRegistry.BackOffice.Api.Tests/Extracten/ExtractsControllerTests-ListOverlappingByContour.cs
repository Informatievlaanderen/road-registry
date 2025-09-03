namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using System.Threading.Tasks;
using Abstractions.Extracts;
using Api.Extracten;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite.Geometries;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task WhenGettingOverlappingExtractsByContour_ThenSucceeded()
    {
        // Arrange
        var overlappingResponse = new GetOverlappingExtractsResponse
        {
            DownloadIds = [Fixture.Create<DownloadId>()]
        };

        Mediator
            .Setup(x => x.Send(It.IsAny<GetOverlappingExtractsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(overlappingResponse);

        var validator = new ExtractenController.GetOverlappingPerContourBodyValidator();

        // Act
        var result = await Controller.GetOverlappingPerContour(
            new ExtractenController.GetOverlappingPerContourBody
            {
                Contour = Polygon.Empty.AsText()
            },
            validator);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<ListOverlappingResponse>(okObjectResult.Value);

        responseObject.DownloadIds.Count.Should().Be(overlappingResponse.DownloadIds.Count);
    }

    [Fact]
    public async Task WhenGettingOverlappingExtractsByContour_WithInvalidRequest_ThenValidationException()
    {
        var validator = new ExtractenController.GetOverlappingPerContourBodyValidator();

        var act = () => Controller.GetOverlappingPerContour(
            new ExtractenController.GetOverlappingPerContourBody(),
            validator);

        await act.Should().ThrowAsync<ValidationException>();
    }
}

public class GetOverlappingPerContourBodyValidatorTests
{
    [Fact]
    public async Task WithEmptyRequest_ThenInvalid()
    {
        var validator = new ExtractenController.GetOverlappingPerContourBodyValidator();

        var sut = await validator.ValidateAsync(new ExtractenController.GetOverlappingPerContourBody());

        sut.IsValid.Should().BeFalse();
        sut.Errors.Should().HaveCount(1);
        sut.Errors.Should().Contain(x => x.PropertyName == "Contour" && x.ErrorMessage == "'Contour' is verplicht.");
    }

    [Fact]
    public async Task WithInvalidContour_ThenInvalid()
    {
        var validator = new ExtractenController.GetOverlappingPerContourBodyValidator();

        var sut = await validator.ValidateAsync(new ExtractenController.GetOverlappingPerContourBody
        {
            Contour = "abc"
        });

        sut.IsValid.Should().BeFalse();
        sut.Errors.Should().HaveCount(1);
        sut.Errors.Should().Contain(x => x.PropertyName == "Contour" && x.ErrorMessage == "'Contour' is geen geldige geometrie.");
    }
}
