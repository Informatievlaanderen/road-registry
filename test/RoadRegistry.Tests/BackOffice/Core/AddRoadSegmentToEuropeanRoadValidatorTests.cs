namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using Xunit;
using AddRoadSegmentToEuropeanRoad = RoadRegistry.BackOffice.Messages.AddRoadSegmentToEuropeanRoad;

public class AddRoadSegmentToEuropeanRoadValidatorTests
{
    public AddRoadSegmentToEuropeanRoadValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizeAttributeId();
        Validator = new AddRoadSegmentToEuropeanRoadValidator();
    }

    public Fixture Fixture { get; }

    [Fact]
    public void RoadNumberMustBeWithinDomain()
    {
        var acceptable = Array.ConvertAll(EuropeanRoadNumber.All, candidate => candidate.ToString());
        var value = new Generator<string>(Fixture).First(candidate => !acceptable.Contains(candidate));
        Validator.ShouldHaveValidationErrorFor(c => c.Number, value);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void TemporaryAttributeIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.TemporaryAttributeId, value);
    }

    public AddRoadSegmentToEuropeanRoadValidator Validator { get; }

    [Fact]
    public void VerifyValid()
    {
        var data = new AddRoadSegmentToEuropeanRoad
        {
            TemporaryAttributeId = Fixture.Create<AttributeId>(),
            Number = EuropeanRoadNumber.All[new Random().Next(0, EuropeanRoadNumber.All.Length)].ToString()
        };

        Validator.ValidateAndThrow(data);
    }
}
