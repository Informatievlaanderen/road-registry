namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.ExtractZipArchiveWriter.V2;

using FluentAssertions;
using RoadRegistry.BackOffice.ZipArchiveWriters.ExtractHost;

public class EditableRoadSegmentsTests
{
    // The example of GAWR-7796: road segments 1-3 are part of an inwinning, the contour overlaps road segments 3-12.
    // Road nodes: A and B are the outer ends of 1 and 2, C joins 1-2-3, K joins 3-4-5, D joins 4-6-7, E and G are the
    // outer ends of 6 and 8, F joins 7-8-9-10, H joins 5-9-11, I joins 10-11-12 and J is the outer end of 12.
    private const int A = 101, B = 102, C = 103, K = 104, D = 105, E = 106, F = 107, G = 108, H = 109, I = 110, J = 111;

    private static readonly IReadOnlyDictionary<int, RoadSegmentTopology> RoadSegments = new[]
    {
        new RoadSegmentTopology(1, A, C),
        new RoadSegmentTopology(2, B, C),
        new RoadSegmentTopology(3, C, K),
        new RoadSegmentTopology(4, K, D),
        new RoadSegmentTopology(5, K, H),
        new RoadSegmentTopology(6, D, E),
        new RoadSegmentTopology(7, D, F),
        new RoadSegmentTopology(8, F, G),
        new RoadSegmentTopology(9, H, F),
        new RoadSegmentTopology(10, F, I),
        new RoadSegmentTopology(11, H, I),
        new RoadSegmentTopology(12, I, J)
    }.ToDictionary(x => x.Id);

    private static IEnumerable<RoadSegmentTopology> Segments(params int[] ids) => ids.Select(id => RoadSegments[id]);

    [Fact]
    public void WhenContourOverlapsWithInwinning_ThenOnlyRoadSegmentsNotAdjacentToInwinningAreEditable()
    {
        var editable = EditableRoadSegments.Determine(
            Segments(3, 4, 5, 6, 7, 8, 9, 10, 11, 12),
            Segments(1, 2, 3));

        editable.Should().BeEquivalentTo([6, 7, 8, 9, 10, 11, 12]);
    }

    [Fact]
    public void WhenNoInwinning_ThenAllRoadSegmentsInContourAreEditable()
    {
        var editable = EditableRoadSegments.Determine(
            Segments(3, 4, 5, 6, 7, 8, 9, 10, 11, 12),
            []);

        editable.Should().BeEquivalentTo([3, 4, 5, 6, 7, 8, 9, 10, 11, 12]);
    }

    [Fact]
    public void WhenInwinningRoadSegmentLiesOutsideContour_ThenAdjacentRoadSegmentInContourIsNotEditable()
    {
        var editable = EditableRoadSegments.Determine(
            Segments(4, 5, 6, 7, 8, 9, 10, 11, 12),
            Segments(3));

        editable.Should().BeEquivalentTo([6, 7, 8, 9, 10, 11, 12]);
    }

    [Fact]
    public void WhenAllRoadSegmentsInContourArePartOfInwinning_ThenNoneAreEditable()
    {
        var editable = EditableRoadSegments.Determine(
            Segments(1, 2, 3),
            Segments(1, 2, 3));

        editable.Should().BeEmpty();
    }
}
