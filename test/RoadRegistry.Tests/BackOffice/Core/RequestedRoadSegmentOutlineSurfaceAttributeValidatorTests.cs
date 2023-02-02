namespace RoadRegistry.Tests.BackOffice.Core;

using AutoFixture;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Messages;
using Xunit;

public class RequestedRoadSegmentOutlineSurfaceAttributeValidatorTests : ValidatorTest<RequestedRoadSegmentSurfaceAttribute, RequestedRoadSegmentOutlineSurfaceAttributeValidator>
{
    public RequestedRoadSegmentOutlineSurfaceAttributeValidatorTests()
    {
        Fixture.CustomizeAttributeId();
        Fixture.CustomizeRoadSegmentPosition();
        Fixture.CustomizeRoadSegmentOutlineSurfaceType();

        var positionGenerator = new Generator<RoadSegmentPosition>(Fixture);
        var from = positionGenerator.First(candidate => candidate >= 0.0m);

        Model = new RequestedRoadSegmentSurfaceAttribute
        {
            AttributeId = Fixture.Create<AttributeId>(),
            FromPosition = from,
            ToPosition = positionGenerator.First(candidate => candidate > from),
            Type = Fixture.Create<RoadSegmentSurfaceType>()
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

    [Fact]
    public void TypeMustBeWithinDomain()
    {
        ShouldHaveValidationErrorFor(c => c.Type, Fixture.Create<string>());
    }

    [Theory]
    [InlineData("NotApplicable")]
    [InlineData("Unknown")]
    public void TypeMustNotBeUnknownAndNotApplicable(string type)
    {
        var roadSegmentSurfaceType = RoadSegmentSurfaceType.Parse(type);
        ShouldHaveValidationErrorFor(c => c.Type, roadSegmentSurfaceType.ToString());
    }
}
