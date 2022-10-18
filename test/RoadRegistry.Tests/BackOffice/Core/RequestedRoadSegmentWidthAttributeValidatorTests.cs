namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;

public class RequestedRoadSegmentWidthAttributeValidatorTests
{
    public RequestedRoadSegmentWidthAttributeValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizeAttributeId();
        Fixture.CustomizeRoadSegmentPosition();
        Fixture.CustomizeRoadSegmentWidth();
        Validator = new RequestedRoadSegmentWidthAttributeValidator();
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void AttributeIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.AttributeId, value);
    }

    public Fixture Fixture { get; }

    [Theory]
    [MemberData(nameof(DynamicAttributePositionCases.NegativeFromPosition), MemberType = typeof(DynamicAttributePositionCases))]
    public void FromPositionMustBePositive(decimal value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.FromPosition, value);
    }

    [Theory]
    [MemberData(nameof(DynamicAttributePositionCases.ToPositionLessThanFromPosition), MemberType = typeof(DynamicAttributePositionCases))]
    public void ToPositionMustBeGreaterThanFromPosition(decimal from, decimal to)
    {
        var data = new RequestedRoadSegmentWidthAttribute
        {
            FromPosition = from,
            ToPosition = to
        };
        Validator.ShouldHaveValidationErrorFor(c => c.ToPosition, data);
    }

    public RequestedRoadSegmentWidthAttributeValidator Validator { get; }

    [Fact]
    public void VerifyValid()
    {
        var positionGenerator = new Generator<decimal>(Fixture);
        var from = positionGenerator.First(candidate => candidate >= 0.0m);

        var data = new RequestedRoadSegmentWidthAttribute
        {
            AttributeId = Fixture.Create<AttributeId>(),
            FromPosition = from,
            ToPosition = positionGenerator.First(candidate => candidate > from),
            Width = Fixture.Create<RoadSegmentWidth>()
        };

        Validator.ValidateAndThrow(data);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(44)]
    [InlineData(45)]
    [InlineData(-8)]
    [InlineData(-9)]
    public void WidthCanBeBetween0And45OrMinus8OrMinus9(int value)
    {
        Validator.ShouldNotHaveValidationErrorFor(c => c.Width, value);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void WidthMustBeGreaterThanOrEqualToZero(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Width, value);
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(46)]
    public void WidthMustBeLessThanOrEqualTo45(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Width, value);
    }
}
