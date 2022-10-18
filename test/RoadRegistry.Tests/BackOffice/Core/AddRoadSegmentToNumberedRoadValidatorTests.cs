namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using Xunit;
using AddRoadSegmentToNumberedRoad = RoadRegistry.BackOffice.Messages.AddRoadSegmentToNumberedRoad;

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

    [Fact]
    public void DirectionMustBeWithinDomain()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Direction, Fixture.Create<string>());
    }

    public Fixture Fixture { get; }

    [Fact]
    public void Ident8MustBeWithinDomain()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Number, Fixture.Create<string>());
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

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void TemporaryAttributeIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.TemporaryAttributeId, value);
    }

    public AddRoadSegmentToNumberedRoadValidator Validator { get; }

    [Fact]
    public void VerifyValid()
    {
        var data = new AddRoadSegmentToNumberedRoad
        {
            TemporaryAttributeId = Fixture.Create<AttributeId>(),
            Number = Fixture.Create<NumberedRoadNumber>(),
            Direction = Fixture.Create<RoadSegmentNumberedRoadDirection>(),
            Ordinal = Fixture.Create<RoadSegmentNumberedRoadOrdinal>()
        };

        Validator.ValidateAndThrow(data);
    }
}