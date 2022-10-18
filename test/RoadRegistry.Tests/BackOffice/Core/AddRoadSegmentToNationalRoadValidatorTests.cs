namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using Xunit;
using AddRoadSegmentToNationalRoad = RoadRegistry.BackOffice.Messages.AddRoadSegmentToNationalRoad;

public class AddRoadSegmentToNationalRoadValidatorTests
{
    public AddRoadSegmentToNationalRoadValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizeAttributeId();
        Fixture.CustomizeNationalRoadNumber();
        Validator = new AddRoadSegmentToNationalRoadValidator();
    }

    public Fixture Fixture { get; }

    [Fact]
    public void Ident2MustBeWithinDomain()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Number, Fixture.Create<string>());
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void TemporaryAttributeIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.TemporaryAttributeId, value);
    }

    public AddRoadSegmentToNationalRoadValidator Validator { get; }

    [Fact]
    public void VerifyValid()
    {
        var data = new AddRoadSegmentToNationalRoad
        {
            TemporaryAttributeId = Fixture.Create<AttributeId>(),
            Number = Fixture.Create<NationalRoadNumber>()
        };

        Validator.ValidateAndThrow(data);
    }
}
