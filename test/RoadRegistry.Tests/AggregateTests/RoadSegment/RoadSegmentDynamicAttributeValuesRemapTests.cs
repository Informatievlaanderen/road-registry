namespace RoadRegistry.Tests.AggregateTests.RoadSegment;

using FluentAssertions;
using RoadRegistry.RoadSegment.ValueObjects;
using RoadRegistry.ValueObjects;

public class RoadSegmentDynamicAttributeValuesRemapTests
{
    // A road segment with three vertices, dynamically segmented into four coverages. Only the stretch between the end
    // vertex that is dragged and the vertex next to it changes length; the coverages on the other side of that vertex
    // stay exactly where they are.
    private static RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection> BuildFourCoverages()
    {
        return new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>()
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(2), RoadSegmentTrafficDirection.Forward)
            .Add(new RoadSegmentPositionV2(2), new RoadSegmentPositionV2(4), RoadSegmentTrafficDirection.Backward)
            .Add(new RoadSegmentPositionV2(4), new RoadSegmentPositionV2(6), RoadSegmentTrafficDirection.Forward)
            .Add(new RoadSegmentPositionV2(6), new RoadSegmentPositionV2(8), RoadSegmentTrafficDirection.Backward);
    }

    [Fact]
    public void WhenTheEndVertexIsDragged_ThenOnlyTheCoveragesPastTheLastVertexFollow()
    {
        // Vertices at 0, 4 and 8; the end vertex moves out to 12, so the 4m last stretch becomes 8m.
        var remapped = BuildFourCoverages().RemapTo([0, 4, 8], [0, 4, 12]);

        var values = remapped.Values.OrderBy(x => x.Coverage.From).ToArray();
        values.Should().HaveCount(4);

        // Before the middle vertex nothing moved at all.
        values[0].Coverage.From.ToDouble().Should().Be(0);
        values[0].Coverage.To.ToDouble().Should().Be(2);
        values[1].Coverage.From.ToDouble().Should().Be(2);
        values[1].Coverage.To.ToDouble().Should().Be(4);

        // Past it the two coverages share the stretch that doubled, so each doubles too.
        values[2].Coverage.From.ToDouble().Should().Be(4);
        values[2].Coverage.To.ToDouble().Should().Be(8);
        values[3].Coverage.From.ToDouble().Should().Be(8);
        values[3].Coverage.To.ToDouble().Should().Be(12);
    }

    [Fact]
    public void WhenTheStartVertexIsDragged_ThenOnlyTheCoveragesBeforeTheSecondVertexAreRescaled()
    {
        // The mirror image: the start vertex is dragged 4m away, so the first stretch doubles from 4m to 8m and
        // everything past the middle vertex keeps its own length and simply shifts along.
        var remapped = BuildFourCoverages().RemapTo([0, 4, 8], [0, 8, 12]);

        var values = remapped.Values.OrderBy(x => x.Coverage.From).ToArray();
        values.Should().HaveCount(4);

        values[0].Coverage.From.ToDouble().Should().Be(0);
        values[0].Coverage.To.ToDouble().Should().Be(4);
        values[1].Coverage.From.ToDouble().Should().Be(4);
        values[1].Coverage.To.ToDouble().Should().Be(8);

        // 2m each, exactly as before, only 4m further along.
        values[2].Coverage.From.ToDouble().Should().Be(8);
        values[2].Coverage.To.ToDouble().Should().Be(10);
        values[3].Coverage.From.ToDouble().Should().Be(10);
        values[3].Coverage.To.ToDouble().Should().Be(12);
    }

    [Fact]
    public void WhenTheFirstStretchGrows_ThenTheAnalysisExampleComesOut()
    {
        // Road segment 2 from the analysis: node K at 0, vertex X at 45, far end at 100. K is dragged so K-X becomes
        // 60m, and the segmentation inside it follows proportionally: 20 x (60/45) = 26.67.
        var remapped = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>()
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(20), RoadSegmentTrafficDirection.Forward)
            .Add(new RoadSegmentPositionV2(20), new RoadSegmentPositionV2(45), RoadSegmentTrafficDirection.Backward)
            .Add(new RoadSegmentPositionV2(45), new RoadSegmentPositionV2(100), RoadSegmentTrafficDirection.Both)
            .RemapTo([0, 45, 100], [0, 60, 115]);

        var values = remapped.Values.OrderBy(x => x.Coverage.From).ToArray();
        values[0].Coverage.To.ToDouble().Should().Be(26.67);
        values[1].Coverage.To.ToDouble().Should().Be(60);

        // Past X nothing was stretched: still 55m of road, shifted by the 15m the first stretch gained.
        values[2].Coverage.From.ToDouble().Should().Be(60);
        values[2].Coverage.To.ToDouble().Should().Be(115);
    }

    [Fact]
    public void WhenTheAffectedStretchShrinks_ThenItsCoveragesShrinkWithIt()
    {
        var remapped = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>()
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(20), RoadSegmentTrafficDirection.Forward)
            .Add(new RoadSegmentPositionV2(20), new RoadSegmentPositionV2(45), RoadSegmentTrafficDirection.Backward)
            .Add(new RoadSegmentPositionV2(45), new RoadSegmentPositionV2(100), RoadSegmentTrafficDirection.Both)
            .RemapTo([0, 45, 100], [0, 30, 85]);

        var values = remapped.Values.OrderBy(x => x.Coverage.From).ToArray();
        values[0].Coverage.To.ToDouble().Should().Be(13.33);
        values[1].Coverage.To.ToDouble().Should().Be(30);
        values[2].Coverage.From.ToDouble().Should().Be(30);
        values[2].Coverage.To.ToDouble().Should().Be(85);
    }

    [Fact]
    public void WhenACoverageSpansTheVertex_ThenItStillCoversTheWholeSegment()
    {
        // One coverage over everything has to keep covering everything, however the stretches on either side of the
        // vertex changed.
        var remapped = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>()
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(8), RoadSegmentTrafficDirection.Forward)
            .RemapTo([0, 4, 8], [0, 4, 12]);

        remapped.Values.Should().ContainSingle();
        remapped.Values[0].Coverage.From.ToDouble().Should().Be(0);
        remapped.Values[0].Coverage.To.ToDouble().Should().Be(12);
    }

    [Fact]
    public void WhenTheSegmentHasASingleStretch_ThenEverythingIsRescaled()
    {
        // A two-vertex segment is nothing but the affected stretch, so the whole segmentation follows it.
        var remapped = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>()
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(20), RoadSegmentTrafficDirection.Forward)
            .Add(new RoadSegmentPositionV2(20), new RoadSegmentPositionV2(45), RoadSegmentTrafficDirection.Backward)
            .RemapTo([0, 45], [0, 60]);

        var values = remapped.Values.OrderBy(x => x.Coverage.From).ToArray();
        values[0].Coverage.To.ToDouble().Should().Be(26.67);
        values[1].Coverage.To.ToDouble().Should().Be(60);
    }

    [Fact]
    public void TheTrailingPositionLandsExactlyOnTheNewLength()
    {
        // Remapping every position individually and rounding each to the centimetre leaves the last one a fraction
        // off; it has to end up on the geometry length or every later change to the segment is rejected.
        var remapped = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>()
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(33.33), RoadSegmentTrafficDirection.Forward)
            .Add(new RoadSegmentPositionV2(33.33), new RoadSegmentPositionV2(100), RoadSegmentTrafficDirection.Backward)
            .RemapTo([0, 100], [0, 100.4987562112089]);

        remapped.Values[^1].Coverage.To.ToDouble().Should().Be(100.5);
    }

    [Fact]
    public void WhenNoVertexMoved_ThenNothingChanges()
    {
        var values = BuildFourCoverages();

        values.RemapTo([0, 4, 8], [0, 4, 8]).Should().Be(values);
    }

    [Fact]
    public void WhenTheVertexCountDoesNotMatch_ThenNothingChanges()
    {
        // Only end vertices move, so the counts always match; a mismatch is not a drag and must not be remapped
        // against a line it does not describe.
        var values = BuildFourCoverages();

        values.RemapTo([0, 4, 8], [0, 12]).Should().Be(values);
    }

    [Fact]
    public void RemappingEmptyValues_ChangesNothing()
    {
        var values = new RoadSegmentDynamicAttributeValues<RoadSegmentTrafficDirection>();

        values.RemapTo([0, 4, 8], [0, 4, 12]).Values.Should().BeEmpty();
    }

    [Fact]
    public void RemappingKeepsSidedValuesOnTheirSide()
    {
        var remapped = new RoadSegmentDynamicAttributeValues<StreetNameLocalId>()
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(45), RoadSegmentAttributeSide.Links, new StreetNameLocalId(1))
            .Add(new RoadSegmentPositionV2(0), new RoadSegmentPositionV2(45), RoadSegmentAttributeSide.Rechts, new StreetNameLocalId(2))
            .RemapTo([0, 45], [0, 60]);

        remapped.Values.Should().HaveCount(2);
        remapped.Values.Should().OnlyContain(x => x.Coverage.To.ToDouble() == 60);
        remapped.Values.Should().ContainSingle(x => x.Side == RoadSegmentAttributeSide.Links && x.Value == new StreetNameLocalId(1));
        remapped.Values.Should().ContainSingle(x => x.Side == RoadSegmentAttributeSide.Rechts && x.Value == new StreetNameLocalId(2));
    }
}
