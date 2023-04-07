namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;

public class RequestedRoadSegmentOutlineLaneAttributeValidatorTests : ValidatorTest<RequestedRoadSegmentLaneAttribute, RequestedRoadSegmentOutlineLaneAttributeValidator>
{
    public RequestedRoadSegmentOutlineLaneAttributeValidatorTests()
    {
        Fixture.CustomizeAttributeId();
        Fixture.CustomizeRoadSegmentPosition();
        Fixture.CustomizeRoadSegmentLaneDirection();
        Fixture.CustomizeRoadSegmentOutlineLaneCount();

        var positionGenerator = new Generator<decimal>(Fixture);
        var from = positionGenerator.First(candidate => candidate >= 0.0m);

        Model = new RequestedRoadSegmentLaneAttribute
        {
            AttributeId = Fixture.Create<AttributeId>(),
            FromPosition = from,
            ToPosition = positionGenerator.First(candidate => candidate > from),
            Count = Fixture.Create<RoadSegmentLaneCount>(),
            Direction = Fixture.Create<RoadSegmentLaneDirection>()
        };
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(-8)]
    [InlineData(-9)]
    public void AttributeIdMustBeGreaterThan(int value)
    {
        ShouldHaveValidationErrorFor(c => c.AttributeId, value);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(7)]
    public void CountCanBeBetween0And7(int value)
    {
        ShouldNotHaveValidationErrorFor(c => c.Count, value);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    [InlineData(0)]
    public void CountMustBeGreaterThanZero(int value)
    {
        ShouldHaveValidationErrorFor(c => c.Count, value);
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(11)]
    public void CountMustBeLessThanOrEqualTo10(int value)
    {
        ShouldHaveValidationErrorFor(c => c.Count, value);
    }

    [Fact]
    public void DirectionMustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.Direction, Fixture.Create<string>());
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
}
