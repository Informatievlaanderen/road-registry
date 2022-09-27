namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using Xunit;

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

    public AddRoadSegmentToNationalRoadValidator Validator { get; }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void TemporaryAttributeIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.TemporaryAttributeId, value);
    }

    [Fact]
    public void Ident2MustBeWithinDomain()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Number, Fixture.Create<string>());
    }

    [Fact]
    public void VerifyValid()
    {
        var data = new RoadRegistry.BackOffice.Messages.AddRoadSegmentToNationalRoad
        {
            TemporaryAttributeId = Fixture.Create<AttributeId>(),
            Number = Fixture.Create<NationalRoadNumber>()
        };

        Validator.ValidateAndThrow(data);
    }
}
