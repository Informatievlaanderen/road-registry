namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using AutoFixture;
using FluentAssertions;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentDynamicAttributeValuesTests
{
    private const string AttributeName = "a";
    private static readonly RoadSegmentId RoadSegmentId = new(1);
    private Fixture _fixture;

    public RoadSegmentDynamicAttributeValuesTests()
    {
        _fixture = new Fixture();
    }

    [Fact]
    public void WhenFromAndToAndSideIsNull_ThenOnly1ValueIsAllowed()
    {
        Validate([
            (null, null, null, 1),
            (null, null, null, 2),
        ]).OfType<Error>().Should().HaveCount(1);

        Validate([
            (null, null, null, 1)
        ]).Should().BeEmpty();
    }

    [Fact]
    public void WhenFromAndToIsNull_Then1ValuePerSideIsAllowed()
    {
        var attributes = new RoadSegmentDynamicAttributeValues<object>();

        var problems = attributes.Validate(RoadSegmentId, AttributeName, 0);
        problems.HasError().Should().BeTrue();
    }

    [Fact]
    public void WhenOnlyFromOrToIsNull_ThenError()
    {
        var attributes = new RoadSegmentDynamicAttributeValues<object>();

        var problems = attributes.Validate(RoadSegmentId, AttributeName, 0);
        problems.HasError().Should().BeTrue();
    }

    [Fact]
    public void WhenFromAndToAreNotNull_ThenFirstFromMustBeZero()
    {
        var attributes = new RoadSegmentDynamicAttributeValues<object>();

        var problems = attributes.Validate(RoadSegmentId, AttributeName, 0);
        problems.HasError().Should().BeTrue();
    }

    [Fact]
    public void WhenFromAndToAreNotNull_ThenFromAndToMustBeDifferent()
    {
        var attributes = new RoadSegmentDynamicAttributeValues<object>();

        var problems = attributes.Validate(RoadSegmentId, AttributeName, 0);
        problems.HasError().Should().BeTrue();
    }

    [Fact]
    public void WhenFromAndToAreNotNull_ThenFromAndToMustBeAdjacent()
    {
        var attributes = new RoadSegmentDynamicAttributeValues<object>();

        var problems = attributes.Validate(RoadSegmentId, AttributeName, 0);
        problems.HasError().Should().BeTrue();
    }

    [Fact]
    public void WhenFromAndToAreNotNull_ThenLastToMustBeEqualToSegmentLength()
    {
        var attributes = new RoadSegmentDynamicAttributeValues<object>();

        var problems = attributes.Validate(RoadSegmentId, AttributeName, 0);
        problems.HasError().Should().BeTrue();
    }

    private Problems Validate((decimal? From, decimal? To, RoadSegmentAttributeSide? Side, object Value)[] attributeValues, double segmentLength = 0)
    {
        var attributes = new RoadSegmentDynamicAttributeValues<object>();

        foreach (var attributeValue in attributeValues)
        {
            var from = attributeValue.From is not null
                ? new RoadSegmentPosition(attributeValue.From.Value)
                : (RoadSegmentPosition?)null;
            var to = attributeValue.To is not null
                ? new RoadSegmentPosition(attributeValue.To.Value)
                : (RoadSegmentPosition?)null;
            attributes.Add(from, to, attributeValue.Side ?? RoadSegmentAttributeSide.Both, attributeValue.Value);
        }

        var problems = attributes.Validate(RoadSegmentId, AttributeName, segmentLength);
        return problems;
    }
}
