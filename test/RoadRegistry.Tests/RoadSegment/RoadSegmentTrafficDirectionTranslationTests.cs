namespace RoadRegistry.Tests.RoadSegment;

using System.Linq;
using RoadRegistry.RoadSegment;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

public class RoadSegmentTrafficDirectionTranslationTests
{
    [Theory]
    [InlineData(true, true, "Both")]
    [InlineData(true, false, "Forward")]
    [InlineData(false, true, "Backward")]
    [InlineData(false, false, "None")]
    public void FromAccess_MapsForwardAndBackwardToDirection(bool forward, bool backward, string expected)
    {
        Assert.Equal(expected, RoadSegmentTrafficDirection.FromAccess(forward, backward).ToString());
    }

    [Theory]
    [InlineData(true, "Both")]
    [InlineData(false, "None")]
    public void PedestrianFromAccess_MapsAccessToDirection(bool access, string expected)
    {
        Assert.Equal(expected, RoadSegmentPedestrianTrafficDirection.FromAccess(access).ToString());
    }

    [Fact]
    public void ToTrafficDirection_WholeSegment_ProducesSingleValue()
    {
        var forward = WholeSegment(true);
        var backward = WholeSegment(false);

        var result = RoadSegmentTrafficDirectionTranslation.ToTrafficDirection(forward, backward);

        var value = Assert.Single(result.Values);
        Assert.Equal(RoadSegmentTrafficDirection.Forward, value.Value);
        Assert.Equal(RoadSegmentPositionV2.Zero, value.Coverage.From);
        Assert.Equal(new RoadSegmentPositionV2(100), value.Coverage.To);
    }

    [Fact]
    public void ToTrafficDirection_WithDifferentBreakpoints_MergesPerInterval()
    {
        // forward: [0,50] = true, [50,100] = false
        var forward = new RoadSegmentDynamicAttributeValues<bool>()
            .Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(50), true)
            .Add(new RoadSegmentPositionV2(50), new RoadSegmentPositionV2(100), false);
        // backward: [0,100] = true
        var backward = WholeSegment(true);

        var result = RoadSegmentTrafficDirectionTranslation.ToTrafficDirection(forward, backward);

        Assert.Equal(2, result.Values.Count);
        var first = result.Values.Single(x => x.Coverage.From == RoadSegmentPositionV2.Zero);
        var second = result.Values.Single(x => x.Coverage.From == new RoadSegmentPositionV2(50));
        Assert.Equal(RoadSegmentTrafficDirection.Both, first.Value);     // forward true + backward true
        Assert.Equal(RoadSegmentTrafficDirection.Backward, second.Value); // forward false + backward true
    }

    [Fact]
    public void ToPedestrianTrafficDirection_MapsEachValue()
    {
        var access = WholeSegment(true);

        var result = RoadSegmentTrafficDirectionTranslation.ToPedestrianTrafficDirection(access);

        Assert.Equal(RoadSegmentPedestrianTrafficDirection.Both, Assert.Single(result.Values).Value);
    }

    private static RoadSegmentDynamicAttributeValues<bool> WholeSegment(bool value)
        => new RoadSegmentDynamicAttributeValues<bool>().Add(RoadSegmentPositionV2.Zero, new RoadSegmentPositionV2(100), value);
}
