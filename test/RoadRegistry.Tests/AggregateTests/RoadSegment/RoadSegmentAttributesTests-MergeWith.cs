namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using FluentAssertions;
using Newtonsoft.Json;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentAttributesMergeWithTests
{
    private readonly ITestOutputHelper _testOutputHelper;

    public RoadSegmentAttributesMergeWithTests(ITestOutputHelper testOutputHelper)
    {
        _testOutputHelper = testOutputHelper;
    }

    [Theory]
    [MemberData(nameof(MergeWithTestsCases))]
    public void WhenMergeWith_ThenExpected(MergeWithTestCase testCase)
    {
        _testOutputHelper.WriteLine($"TestCase: {JsonConvert.SerializeObject(testCase, Formatting.Indented)}");

        var attributes1 = testCase.Segment1Attributes.ToRoadSegmentDynamicAttributeValues();
        var attributes2 = testCase.Segment2Attributes.ToRoadSegmentDynamicAttributeValues();

        var mergedAttributes = attributes1.MergeWith(attributes2, testCase.Segment1Length, testCase.Segment2Length, testCase.Segment1HasIdealDirection, testCase.Segment2HasIdealDirection);
        mergedAttributes.Values.Should().HaveCount(testCase.ExpectedAttributes.Values.Count);

        foreach (var (expected, index) in testCase.ExpectedAttributes.Values.Select((x, i) => (x, i)))
        {
            var actual = mergedAttributes.Values[index];
            ((int)actual.Coverage.From.ToDecimal()).Should().Be(expected.From);
            ((int)actual.Coverage.To.ToDecimal()).Should().Be(expected.To);
            actual.Side.Should().Be(expected.Side);
            actual.Value.Should().Be(expected.Value);
        }
    }
    public static IEnumerable<object[]> MergeWithTestsCases()
    {
        yield return [new MergeWithTestCase("single attribute with identical value over entire segment")
        {
            Segment1Length = 10,
            Segment1Attributes = new AttributeValues([(0, 10, "a")]),
            Segment2Length = 5,
            Segment2Attributes = new AttributeValues([(0, 5, "a")]),
            ExpectedAttributes = new AttributeValues([(0, 15, "a")])
        }];
        yield return [new MergeWithTestCase("single attribute with identical value over entire segment and non-Both sides")
        {
            Segment1Length = 10,
            Segment1Attributes = new AttributeValues([(0, 10, "a")]),
            Segment2Length = 5,
            Segment2Attributes = new AttributeValues([(0, 5, RoadSegmentAttributeSide.Left, "a")]),
            ExpectedAttributes = new AttributeValues([(0, 10, RoadSegmentAttributeSide.Both, "a"), (10, 15, RoadSegmentAttributeSide.Left, "a")])
        }];
        yield return [new MergeWithTestCase("single attribute with different values over entire segment")
        {
            Segment1Length = 10,
            Segment1Attributes = new AttributeValues([(0, 10, "a")]),
            Segment2Length = 5,
            Segment2Attributes = new AttributeValues([(0, 5, "b")]),
            ExpectedAttributes = new AttributeValues([(0, 10, "a"), (10, 15, "b")])
        }];
        yield return [new MergeWithTestCase("single attribute with different values and non-Both side")
        {
            Segment1Length = 10,
            Segment1Attributes = new AttributeValues([(0, 10, "a")]),
            Segment2Length = 5,
            Segment2Attributes = new AttributeValues([(0, 5, RoadSegmentAttributeSide.Left, "b")]),
            ExpectedAttributes = new AttributeValues([(0, 10, RoadSegmentAttributeSide.Both, "a"), (10, 15, RoadSegmentAttributeSide.Left, "b")])
        }];

        yield return [new MergeWithTestCase("multiple attributes with both ideal direction")
        {
            Segment1Length = 10,
            Segment1Attributes = new AttributeValues([(0, 2, "a"), (2, 10, "b")]),
            Segment1HasIdealDirection = true,
            Segment2Length = 5,
            Segment2Attributes = new AttributeValues([(0, 2, "a"), (2, 5, "b")]),
            Segment2HasIdealDirection = true,
            ExpectedAttributes = new AttributeValues([(0, 2, "a"), (2, 10, "b"), (10, 12, "a"), (12, 15, "b")])
        }];
        yield return [new MergeWithTestCase("multiple attributes with only segment2 ideal direction")
        {
            Segment1Length = 10,
            Segment1Attributes = new AttributeValues([(0, 2, "a"), (2, 10, "b")]),
            Segment1HasIdealDirection = false,
            Segment2Length = 5,
            Segment2Attributes = new AttributeValues([(0, 2, "a"), (2, 5, "b")]),
            Segment2HasIdealDirection = true,
            ExpectedAttributes = new AttributeValues([(0, 8, "b"), (8, 12, "a"), (12, 15, "b")])
        }];
        yield return [new MergeWithTestCase("multiple attributes with only segment1 ideal direction")
        {
            Segment1Length = 10,
            Segment1Attributes = new AttributeValues([(0, 2, "a"), (2, 10, "b")]),
            Segment1HasIdealDirection = true,
            Segment2Length = 5,
            Segment2Attributes = new AttributeValues([(0, 2, "a"), (2, 5, "b")]),
            Segment2HasIdealDirection = false,
            ExpectedAttributes = new AttributeValues([(0, 2, "a"), (2, 13, "b"), (13, 15, "a")])
        }];
        yield return [new MergeWithTestCase("multiple attributes with none ideal direction")
        {
            Segment1Length = 10,
            Segment1Attributes = new AttributeValues([(0, 2, "a"), (2, 10, "b")]),
            Segment1HasIdealDirection = false,
            Segment2Length = 5,
            Segment2Attributes = new AttributeValues([(0, 2, "a"), (2, 5, "b")]),
            Segment2HasIdealDirection = false,
            ExpectedAttributes = new AttributeValues([(0, 8, "b"), (8, 10, "a"), (10, 13, "b"), (13, 15, "a")])
        }];
    }

    public sealed class MergeWithTestCase
    {
        public string Description { get; init; }
        public double Segment1Length { get; init; }
        public bool Segment1HasIdealDirection { get; init; } = true;
        public double Segment2Length { get; init; }
        public bool Segment2HasIdealDirection { get; init; } = true;
        public AttributeValues Segment1Attributes { get; init; }
        public AttributeValues Segment2Attributes { get; init; }
        public AttributeValues ExpectedAttributes { get; init; }

        public MergeWithTestCase(string description)
        {
            Description = description;
        }
    }
    public sealed class AttributeValues
    {
        public ICollection<(int From, int To, RoadSegmentAttributeSide Side, string Value)> Values { get; }

        public AttributeValues(ICollection<(int, int, string)> values)
            : this(values.Select(x => (x.Item1, x.Item2, RoadSegmentAttributeSide.Both, x.Item3)).ToArray())
        {
        }
        public AttributeValues(ICollection<(int, int, RoadSegmentAttributeSide, string)> values)
        {
            Values = values.Select(x => (x.Item1, x.Item2, x.Item3, x.Item4)).ToArray();
        }

        public RoadSegmentDynamicAttributeValues<string> ToRoadSegmentDynamicAttributeValues()
        {
            return new RoadSegmentDynamicAttributeValues<string>(Values.Select(x => (
                new RoadSegmentPositionCoverage(new RoadSegmentPosition(x.Item1), new RoadSegmentPosition(x.Item2)),
                x.Item3,
                x.Item4)));
        }
    }
}
