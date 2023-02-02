namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;

public class RequestedRoadSegmentOutlineWidthAttributeValidatorTests : ValidatorTest<RequestedRoadSegmentWidthAttribute, RequestedRoadSegmentOutlineWidthAttributeValidator>
{
    public RequestedRoadSegmentOutlineWidthAttributeValidatorTests()
    {
        Fixture.CustomizeAttributeId();
        Fixture.CustomizeRoadSegmentPosition();
        Fixture.CustomizeRoadSegmentOutlineWidth();

        var positionGenerator = new Generator<decimal>(Fixture);
        var from = positionGenerator.First(candidate => candidate >= 0.0m);

        Model = new RequestedRoadSegmentWidthAttribute
        {
            AttributeId = Fixture.Create<AttributeId>(),
            FromPosition = from,
            ToPosition = positionGenerator.First(candidate => candidate > from),
            Width = Fixture.Create<RoadSegmentWidth>()
        };
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void AttributeIdMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.AttributeId, value);
    }

    [Theory]
    [MemberData(nameof(DynamicAttributePositionCases.NegativeFromPosition), MemberType = typeof(DynamicAttributePositionCases))]
    public void FromPositionMustBePositive(decimal value)
    {
        ShouldHaveValidationErrorFor(c => c.FromPosition, value);
    }

    [Theory]
    [MemberData(nameof(DynamicAttributePositionCases.ToPositionLessThanFromPosition), MemberType = typeof(DynamicAttributePositionCases))]
    public void ToPositionMustBeGreaterThanFromPosition(decimal to)
    {
        ShouldHaveValidationErrorFor(c => c.ToPosition, to);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(44)]
    [InlineData(45)]
    public void WidthCanBeBetween0And45OrMinus8OrMinus9(int value)
    {
        ShouldNotHaveValidationErrorFor(c => c.Width, value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(-8)]
    [InlineData(-9)]
    public void WidthMustBeGreaterThanZero(int value)
    {
        ShouldHaveValidationErrorFor(c => c.Width, value);
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(46)]
    public void WidthMustBeLessThanOrEqualTo45(int value)
    {
        ShouldHaveValidationErrorFor(c => c.Width, value);
    }
}
