namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using FluentValidation;
using FluentValidation.TestHelper;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;

public class RequestedRoadSegmentLaneAttributeValidatorTests
{
    public RequestedRoadSegmentLaneAttributeValidatorTests()
    {
        Fixture = new Fixture();
        Fixture.CustomizeAttributeId();
        Fixture.CustomizeRoadSegmentPosition();
        Fixture.CustomizeRoadSegmentLaneCount();
        Fixture.CustomizeRoadSegmentLaneDirection();
        Validator = new RequestedRoadSegmentLaneAttributeValidator();
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void AttributeIdMustBeGreaterThan(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.AttributeId, value);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(6)]
    [InlineData(7)]
    [InlineData(-8)]
    [InlineData(-9)]
    public void CountCanBeBetween0And7OrMinus8OrMinus9(int value)
    {
        Validator.ShouldNotHaveValidationErrorFor(c => c.Count, value);
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(-1)]
    public void CountMustBeGreaterThanOrEqualToZero(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Count, value);
    }

    [Theory]
    [InlineData(int.MaxValue)]
    [InlineData(8)]
    public void CountMustBeLessThanOrEqualTo7(int value)
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Count, value);
    }

    [Fact]
    public void DirectionMustBeWithinDomain()
    {
        Validator.ShouldHaveValidationErrorFor(c => c.Direction, Fixture.Create<string>());
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
        var data = new RequestedRoadSegmentLaneAttribute
        {
            FromPosition = from,
            ToPosition = to
        };
        Validator.ShouldHaveValidationErrorFor(c => c.ToPosition, data);
    }

    public RequestedRoadSegmentLaneAttributeValidator Validator { get; }

    [Fact]
    public void VerifyValid()
    {
        var positionGenerator = new Generator<decimal>(Fixture);
        var from = positionGenerator.First(candidate => candidate >= 0.0m);

        var data = new RequestedRoadSegmentLaneAttribute
        {
            AttributeId = Fixture.Create<AttributeId>(),
            FromPosition = from,
            ToPosition = positionGenerator.First(candidate => candidate > from),
            Count = Fixture.Create<RoadSegmentLaneCount>(),
            Direction = Fixture.Create<RoadSegmentLaneDirection>()
        };

        Validator.ValidateAndThrow(data);
    }
}