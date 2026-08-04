namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using FluentAssertions;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

public class RoadSegmentDynamicAttributeValuesScaleTests
{
    // The worked example from the analysis: a segment dynamically segmented over 45m, of which the first 20m carry a
    // different value, grows to 60m because the road node it hangs off moved. The division moves proportionally:
    // 20 x (60/45) = 26.67.
    private static RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> Build()
    {
        return new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>()
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(20), RoadSegmentTrafficDirection.Forward)
            .Add(new RoadSegmentPositionV2(20), new RoadSegmentPositionV2(45), RoadSegmentTrafficDirection.Backward);
    }

    [Fact]
    public void ScalingToALongerSegment_MovesEveryPositionProportionally()
    {
        var scaled = Build().ScaleTo(45, 60);

        scaled.Values.Should().HaveCount(2);
        scaled.Values[0].Coverage.From.ToDouble().Should().Be(0);
        scaled.Values[0].Coverage.To.ToDouble().Should().Be(26.67);
        scaled.Values[0].Value.Should().Be(RoadSegmentTrafficDirection.Forward);
        scaled.Values[1].Coverage.From.ToDouble().Should().Be(26.67);
        scaled.Values[1].Coverage.To.ToDouble().Should().Be(60);
        scaled.Values[1].Value.Should().Be(RoadSegmentTrafficDirection.Backward);
    }

    [Fact]
    public void ScalingToAShorterSegment_MovesEveryPositionProportionally()
    {
        var scaled = Build().ScaleTo(45, 30);

        scaled.Values.Should().HaveCount(2);
        scaled.Values[0].Coverage.To.ToDouble().Should().Be(13.33);
        scaled.Values[1].Coverage.From.ToDouble().Should().Be(13.33);
        scaled.Values[1].Coverage.To.ToDouble().Should().Be(30);
    }

    [Fact]
    public void TheTrailingPositionLandsExactlyOnTheNewLength()
    {
        // Scaling every position individually and rounding each to the centimetre leaves the last one a fraction off;
        // it has to end up on the geometry length or every later change to the segment is rejected.
        var scaled = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>()
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(33.33), RoadSegmentTrafficDirection.Forward)
            .Add(new RoadSegmentPositionV2(33.33), new RoadSegmentPositionV2(100), RoadSegmentTrafficDirection.Backward)
            .ScaleTo(100, 100.4987562112089);

        scaled.Values[^1].Coverage.To.ToDouble().Should().Be(100.5);
    }

    [Fact]
    public void ScalingToTheSameLength_ChangesNothing()
    {
        var values = Build();

        var scaled = values.ScaleTo(45, 45);

        scaled.Should().Be(values);
    }

    [Fact]
    public void ScalingEmptyValues_ChangesNothing()
    {
        var values = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>();

        values.ScaleTo(45, 60).Values.Should().BeEmpty();
    }

    [Fact]
    public void ScalingKeepsSidedValuesOnTheirSide()
    {
        var scaled = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(45), RoadSegmentAttributeSide.Links, new StreetNameLocalId(1))
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(45), RoadSegmentAttributeSide.Rechts, new StreetNameLocalId(2))
            .ScaleTo(45, 60);

        scaled.Values.Should().HaveCount(2);
        scaled.Values.Should().OnlyContain(x => x.Coverage.To.ToDouble() == 60);
        scaled.Values.Should().ContainSingle(x => x.Side == RoadSegmentAttributeSide.Links && x.Value == new StreetNameLocalId(1));
        scaled.Values.Should().ContainSingle(x => x.Side == RoadSegmentAttributeSide.Rechts && x.Value == new StreetNameLocalId(2));
    }
}
