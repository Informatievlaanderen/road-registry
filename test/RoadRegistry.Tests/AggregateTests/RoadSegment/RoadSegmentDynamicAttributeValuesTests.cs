namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using FluentAssertions;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core.ProblemCodes;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentDynamicAttributeValuesTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RoadSegmentDynamicAttributeValuesTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void WhenFromAndToAreNullAndSideIsBoth_ThenOnly1ValueIsAllowed()
    {
        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Both, 1)
        ], expectedErrorCodes: []);

        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Both, 1),
            (null, null, RoadSegmentAttributeSide.Both, 2),
        ], expectedErrorCodes: ["RoadSegmentCategoryValueNotUniqueWithinSegment"]);
    }

    [Fact]
    public void WhenFromAndToIsNullAndSideIsNotNull_Then1ValuePerSideIsAllowed()
    {
        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Both, 1)
        ], expectedErrorCodes: []);

        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Left, 1)
        ], expectedErrorCodes: []);

        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Left, 1),
            (null, null, RoadSegmentAttributeSide.Right, 1)
        ], expectedErrorCodes: []);

        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Both, 1),
            (null, null, RoadSegmentAttributeSide.Both, 2)
        ], expectedErrorCodes: ["RoadSegmentCategoryValueNotUniqueWithinSegment"]);

        AssertValidateResult([
            (null, null, RoadSegmentAttributeSide.Left, 1),
            (null, null, RoadSegmentAttributeSide.Left, 2)
        ], expectedErrorCodes: ["RoadSegmentCategoryValueNotUniqueWithinSegment"]);
    }

    [Fact]
    public void WhenOnlyFromOrToIsNull_ThenError()
    {
        AssertValidateResult([
            (0, 5, RoadSegmentAttributeSide.Both, 1)
        ],
            segmentLength: 5,
            expectedErrorCodes: []);

        AssertValidateResult([
            (5, null, RoadSegmentAttributeSide.Both, 1)
        ], expectedErrorCodes: ["RoadSegmentCategoryFromOrToPositionIsNull"]);

        AssertValidateResult([
            (null, 5, RoadSegmentAttributeSide.Both, 1)
        ], expectedErrorCodes: ["RoadSegmentCategoryFromOrToPositionIsNull"]);
    }

    [Fact]
    public void WhenFromAndToAreNotNull_ThenFirstFromMustBeZero()
    {
        AssertValidateResult(
            [
                (1, 5, RoadSegmentAttributeSide.Both, 1)
            ],
            segmentLength: 5,
            expectedErrorCodes: ["RoadSegmentCategoryFromPositionNotEqualToZero"]);
    }

    [Fact]
    public void WhenFromAndToAreNotNull_ThenFromAndToMustBeDifferent()
    {
        AssertValidateResult(
            [
                (0, 1, RoadSegmentAttributeSide.Both, 1),
                (1, 1, RoadSegmentAttributeSide.Both, 2)
            ],
            segmentLength: 1,
            expectedErrorCodes: ["RoadSegmentCategoryHasLengthOfZero"]);
    }

    [Fact]
    public void WhenFromAndToAreNotNull_ThenFromAndToMustBeAdjacent()
    {
        AssertValidateResult(
            [
                (0, 1, RoadSegmentAttributeSide.Both, 1),
                (4, 5, RoadSegmentAttributeSide.Both, 2)
            ],
            segmentLength: 5,
            expectedErrorCodes: ["RoadSegmentCategoryNotAdjacent"]);
    }

    [Fact]
    public void WhenFromAndToAreNotNull_ThenLastToMustBeEqualToSegmentLength()
    {
        AssertValidateResult(
            [
                (0, 1, RoadSegmentAttributeSide.Both, 1),
                (1, 2, RoadSegmentAttributeSide.Both, 2)
            ],
            segmentLength: 5,
            expectedErrorCodes: ["RoadSegmentCategoryToPositionNotEqualToLength"]);
    }

    private void AssertValidateResult((decimal? From, decimal? To, RoadSegmentAttributeSide Side, object Value)[] attributeValues, string[] expectedErrorCodes, double segmentLength = 0)
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
            attributes.Add(from, to, attributeValue.Side, attributeValue.Value);
        }


        var problems = attributes.Validate(new RoadSegmentId(1), segmentLength, ProblemCode.RoadSegment.Category.DynamicAttributeProblemCodes);
        foreach (var problem in problems)
        {
            _testOutputHelper.WriteLine(problem.Describe());
        }

        if (expectedErrorCodes.Any())
        {
            foreach (var errorCode in expectedErrorCodes)
            {
                problems.Should().Contain(x => x.Reason == errorCode);
            }
        }
        else
        {
            problems.Should().BeEmpty();
        }
    }
}
