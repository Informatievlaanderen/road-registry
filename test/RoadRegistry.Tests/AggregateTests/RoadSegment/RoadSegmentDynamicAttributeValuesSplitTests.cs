namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using FluentAssertions;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

public class RoadSegmentDynamicAttributeValuesSplitTests
{
    private static RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> Build()
    {
        return new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>()
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(4), RoadSegmentTrafficDirection.Forward)
            .Add(new RoadSegmentPositionV2(4), new RoadSegmentPositionV2(10), RoadSegmentTrafficDirection.Backward);
    }

    [Fact]
    public void SplitExactlyOnAttributeBoundary_KeepsEachAttributeInItsPart()
    {
        var (first, second) = Build().SplitAt(new RoadSegmentPositionV2(4), 10);

        first.Values.Should().ContainSingle();
        first.Values[0].Coverage.From.ToDouble().Should().Be(0);
        first.Values[0].Coverage.To.ToDouble().Should().Be(4);
        first.Values[0].Value.Should().Be(RoadSegmentTrafficDirection.Forward);

        second.Values.Should().ContainSingle();
        second.Values[0].Coverage.From.ToDouble().Should().Be(0);
        second.Values[0].Coverage.To.ToDouble().Should().Be(6);
        second.Values[0].Value.Should().Be(RoadSegmentTrafficDirection.Backward);
    }

    [Fact]
    public void SplitInsideAnAttribute_ClipsAndRebases()
    {
        var (first, second) = Build().SplitAt(new RoadSegmentPositionV2(6), 10);

        first.Values.Should().HaveCount(2);
        first.Values[0].Coverage.From.ToDouble().Should().Be(0);
        first.Values[0].Coverage.To.ToDouble().Should().Be(4);
        first.Values[0].Value.Should().Be(RoadSegmentTrafficDirection.Forward);
        first.Values[1].Coverage.From.ToDouble().Should().Be(4);
        first.Values[1].Coverage.To.ToDouble().Should().Be(6);
        first.Values[1].Value.Should().Be(RoadSegmentTrafficDirection.Backward);

        // second part rebased to start at 0: [6,10] -> [0,4]
        second.Values.Should().ContainSingle();
        second.Values[0].Coverage.From.ToDouble().Should().Be(0);
        second.Values[0].Coverage.To.ToDouble().Should().Be(4);
        second.Values[0].Value.Should().Be(RoadSegmentTrafficDirection.Backward);
    }

    [Fact]
    public void SplitSingleFullCoverageAttribute_ProducesTwoAdjacentParts()
    {
        var values = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>()
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(10), RoadSegmentTrafficDirection.Both);

        var (first, second) = values.SplitAt(new RoadSegmentPositionV2(3), 10);

        first.Values.Should().ContainSingle();
        first.Values[0].Coverage.From.ToDouble().Should().Be(0);
        first.Values[0].Coverage.To.ToDouble().Should().Be(3);

        second.Values.Should().ContainSingle();
        second.Values[0].Coverage.From.ToDouble().Should().Be(0);
        second.Values[0].Coverage.To.ToDouble().Should().Be(7);
    }
}
