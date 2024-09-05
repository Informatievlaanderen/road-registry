namespace RoadRegistry.BackOffice.Api.Tests.Extracts.ListOverlapping;

using FluentAssertions;
using RoadRegistry.BackOffice.Api.Extracts;

public class WhenByContour
{
    [Fact]
    public async Task WhenListOverlapping_GivenEmptyRequest_ThenInvalid()
    {
        var validator = new ExtractsController.ListOverlappingByContourParametersValidator();

        var sut = await validator.ValidateAsync(new ExtractsController.ListOverlappingByContourParameters());

        sut.IsValid.Should().BeFalse();
        sut.Errors.Should().HaveCount(1);
        sut.Errors.Should().Contain(x => x.PropertyName == "" && x.ErrorMessage == "'Contour' is verplicht.");
    }

    [Fact]
    public async Task WhenListOverlapping_GivenInvalidContour_ThenInvalid()
    {
        var validator = new ExtractsController.ListOverlappingByContourParametersValidator();

        var sut = await validator.ValidateAsync(new ExtractsController.ListOverlappingByContourParameters
        {
            Contour = "abc"
        });

        sut.IsValid.Should().BeFalse();
        sut.Errors.Should().HaveCount(1);
        sut.Errors.Should().Contain(x => x.PropertyName == "Contour" && x.ErrorMessage == "'Contour' is geen geldige geometrie.");
    }
}
