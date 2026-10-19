namespace RoadRegistry.Tests.BackOffice;

using FluentAssertions;
using RoadRegistry.ValueObjects.Problems;

// The identifiers end up in the message the caller reads, so how the list is cut matters: an inwinningszone can hold
// thousands of road segments and naming all of them would be unreadable and unbounded.
public class InwinningRoadSegmentsMissingFromUploadTests
{
    private const string ParameterName = InwinningRoadSegmentsMissingFromUpload.ParameterName.RoadSegmentIds;
    private const int Cap = InwinningRoadSegmentsMissingFromUpload.MaximumReportedRoadSegmentIds;

    [Fact]
    public void WithFewerThanTheCap_ThenAllAreNamed()
    {
        ReportedRoadSegmentIds(RoadSegmentIds(3)).Should().Be("1, 2, 3");
    }

    [Fact]
    public void WithExactlyTheCap_ThenAllAreNamedWithoutARemainder()
    {
        var roadSegmentIds = RoadSegmentIds(Cap);

        ReportedRoadSegmentIds(roadSegmentIds).Should().Be(string.Join(", ", Enumerable.Range(1, Cap)));
    }

    [Fact]
    public void WithMoreThanTheCap_ThenTheRemainderIsCounted()
    {
        var roadSegmentIds = RoadSegmentIds(Cap + 3);

        ReportedRoadSegmentIds(roadSegmentIds).Should().Be($"{string.Join(", ", Enumerable.Range(1, Cap))} en 3 andere");
    }

    private static string? ReportedRoadSegmentIds(IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
    {
        return new InwinningRoadSegmentsMissingFromUpload(roadSegmentIds)
            .Parameters
            .Single(x => x.Name == ParameterName)
            .Value;
    }

    private static RoadSegmentId[] RoadSegmentIds(int count)
    {
        return Enumerable.Range(1, count).Select(x => new RoadSegmentId(x)).ToArray();
    }
}
