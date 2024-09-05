namespace RoadRegistry.BackOffice.Api.Tests.Extracts.ListOverlapping;

using FluentAssertions;
using RoadRegistry.BackOffice.Api.Extracts;

public class WhenByNisCode
{
    [Fact]
    public async Task WhenListOverlapping_GivenEmptyRequest_ThenInvalid()
    {
        var validator = new ExtractsController.ListOverlappingByNisCodeParametersValidator();

        var sut = await validator.ValidateAsync(new ExtractsController.ListOverlappingByNisCodeParameters());

        sut.IsValid.Should().BeFalse();
        sut.Errors.Should().HaveCount(1);
        sut.Errors.Should().Contain(x => x.PropertyName == "NisCode" && x.ErrorMessage == "'NisCode' is verplicht.");
    }

    [Fact]
    public async Task WhenListOverlapping_GivenNonNumericNisCode_ThenInvalid()
    {
        var validator = new ExtractsController.ListOverlappingByNisCodeParametersValidator();

        var sut = await validator.ValidateAsync(new ExtractsController.ListOverlappingByNisCodeParameters
        {
            NisCode = "abc"
        });

        sut.IsValid.Should().BeFalse();
        sut.Errors.Should().HaveCount(1);
        sut.Errors.Should().Contain(x => x.PropertyName == "NisCode" && x.ErrorMessage == "Ongeldige NisCode. Verwacht formaat: '12345'");
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(101)]
    public async Task WhenListOverlapping_GivenInvalidBuffer_ThenInvalid(int buffer)
    {
        var validator = new ExtractsController.ListOverlappingByNisCodeParametersValidator();

        var sut = await validator.ValidateAsync(new ExtractsController.ListOverlappingByNisCodeParameters
        {
            NisCode = "10000",
            Buffer = buffer
        });

        sut.IsValid.Should().BeFalse();
        sut.Errors.Should().HaveCount(1);
        sut.Errors.Should().Contain(x => x.PropertyName == "Buffer" && x.ErrorMessage == "'Buffer' moet een waarde tussen 0 en 100 zijn.");
    }
}
