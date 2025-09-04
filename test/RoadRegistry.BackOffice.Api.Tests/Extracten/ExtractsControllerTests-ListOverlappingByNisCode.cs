namespace RoadRegistry.BackOffice.Api.Tests.Extracten;

using System.Linq;
using Abstractions.Extracts;
using Api.Extracten;
using AutoFixture;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetTopologySuite.Geometries;
using Sync.MunicipalityRegistry;
using Sync.MunicipalityRegistry.Models;

public partial class ExtractsControllerTests
{
    [Fact]
    public async Task WhenGettingOverlappingExtractsByNisCode_ThenSucceeded()
    {
        // Arrange
        var overlappingResponse = new GetOverlappingExtractsResponse
        {
            DownloadIds = [Fixture.Create<DownloadId>()]
        };

        Mediator
            .Setup(x => x.Send(It.IsAny<GetOverlappingExtractsRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(overlappingResponse);

        var validator = new ExtractenController.GetOverlappingPerNisCodeBodyValidator();

        var municipalityContext = _dbContextBuilder.CreateMunicipalityEventConsumerContext();
        municipalityContext.Municipalities.Add(new Municipality
        {
            MunicipalityId = Fixture.Create<string>(),
            NisCode = "12345",
            Geometry = Polygon.Empty,
            Status = MunicipalityStatus.Current
        });
        await municipalityContext.SaveChangesAsync();

        // Act
        var result = await Controller.GetOverlappingPerNisCode(
            new ExtractenController.GetOverlappingPerNisCodeBody
            {
                NisCode = "12345"
            },
            validator,
            municipalityContext);

        // Assert
        var okObjectResult = Assert.IsType<OkObjectResult>(result);
        var responseObject = Assert.IsType<ListOverlappingResponse>(okObjectResult.Value);

        responseObject.DownloadIds.Count.Should().Be(overlappingResponse.DownloadIds.Count);
    }

    [Fact]
    public async Task WhenGettingOverlappingExtractsByNisCode_WithInvalidRequest_ThenValidationException()
    {
        var validator = new ExtractenController.GetOverlappingPerNisCodeBodyValidator();
        var municipalityContext = _dbContextBuilder.CreateMunicipalityEventConsumerContext();

        var act = () => Controller.GetOverlappingPerNisCode(
            new ExtractenController.GetOverlappingPerNisCodeBody(),
            validator,
            municipalityContext);

        await act.Should().ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task WhenGettingOverlappingExtractsByNisCode_WithUnknownNisCode_ThenValidationException()
    {
        var validator = new ExtractenController.GetOverlappingPerNisCodeBodyValidator();
        var municipalityContext = _dbContextBuilder.CreateMunicipalityEventConsumerContext();

        var act = () => Controller.GetOverlappingPerNisCode(
            new ExtractenController.GetOverlappingPerNisCodeBody
            {
                NisCode = "12345"
            },
            validator,
            municipalityContext);

        var ex = (await act.Should().ThrowAsync<ValidationException>()).Which;
        var error = ex.Errors.First();
        error.PropertyName.Should().Be("NisCode");
        error.ErrorCode.Should().Be("NotFound");
        error.ErrorMessage.Should().Be("Er werd geen gemeente/stad gevonden voor de NIS-code '12345'");
    }
}

public class GetOverlappingPerNisCodeBodyValidatorTests
{
    [Fact]
    public async Task WithEmptyRequest_ThenInvalid()
    {
        var validator = new ExtractenController.GetOverlappingPerNisCodeBodyValidator();

        var sut = await validator.ValidateAsync(new ExtractenController.GetOverlappingPerNisCodeBody());

        sut.IsValid.Should().BeFalse();
        sut.Errors.Should().HaveCount(1);
        sut.Errors.Should().Contain(x => x.PropertyName == "NisCode" && x.ErrorMessage == "'NisCode' is verplicht.");
    }

    [Fact]
    public async Task WithNonNumericNisCode_ThenInvalid()
    {
        var validator = new ExtractenController.GetOverlappingPerNisCodeBodyValidator();

        var sut = await validator.ValidateAsync(new ExtractenController.GetOverlappingPerNisCodeBody
        {
            NisCode = "abc"
        });

        sut.IsValid.Should().BeFalse();
        sut.Errors.Should().HaveCount(1);
        sut.Errors.Should().Contain(x => x.PropertyName == "NisCode" && x.ErrorMessage == "Ongeldige NisCode. Verwacht formaat: '12345'");
    }
}
