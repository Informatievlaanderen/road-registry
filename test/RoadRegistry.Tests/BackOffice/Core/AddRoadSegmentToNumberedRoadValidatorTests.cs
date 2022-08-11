namespace RoadRegistry.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using Xunit;

public class AddRoadSegmentToNumberedRoadValidatorTests
{
    public AddRoadSegmentToNumberedRoadValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizeAttributeId();
        Fixture.CustomizeNumberedRoadNumber();
        Fixture.CustomizeRoadSegmentNumberedRoadOrdinal();
        Fixture.CustomizeRoadSegmentNumberedRoadDirection();
        Validator = new AddRoadSegmentToNumberedRoadValidator();
    }

    public Fixture Fixture { get; }

    public AddRoadSegmentToNumberedRoadValidator Validator { get; }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void TemporaryAttributeIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.TemporaryAttributeId, value);
    }

    [Fact]
    public void Ident8MustBeWithinDomain()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Number, Fixture.Create<string>());
    }

    [Fact]
    public void DirectionMustBeWithinDomain()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Direction, Fixture.Create<string>());
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void OrdinalMustBeGreaterThanOrEqualToZero(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Ordinal, value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MaxValue)]
    [InlineData(RoadSegmentNumberedRoadOrdinal.WellKnownValues.NotKnown)]
    public void OrdinalMustBeGreaterThanOrEqualToZeroOrAcceptedValue(int value)
    {
        Validator.ShouldNotHaveValidationErrorFor(c => c.Ordinal, value);
    }

    [Fact]
    public void VerifyValid()
    {
        var data = new Messages.AddRoadSegmentToNumberedRoad
        {
            TemporaryAttributeId = Fixture.Create<AttributeId>(),
            Number = Fixture.Create<NumberedRoadNumber>(),
            Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>(),
            Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>()
        };

        Validator.ValidateAndThrow(data);
    }
}
