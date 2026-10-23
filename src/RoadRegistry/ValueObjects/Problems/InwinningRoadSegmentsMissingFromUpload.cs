namespace RoadRegistry.ValueObjects.Problems;

using System.Collections.Generic;
using System.Linq;
using ProblemCodes;

// An upload that does not account for every road segment its inwinningszone is still waiting on. The caller did
// nothing wrong - this points at the upload losing a road segment somewhere before it got here - but the inwinning
// cannot be completed on it, so it is reported as a validation failure and the upload can be redone.
public class InwinningRoadSegmentsMissingFromUpload : Error
{
    public static class ParameterName
    {
        public const string RoadSegmentIds = "RoadSegmentIds";
    }

    // The identifiers are named to the caller, so the list is capped: an inwinningszone can hold thousands of road
    // segments and a message naming all of them is both unreadable and unbounded. The full list goes to the log.
    public const int MaximumReportedRoadSegmentIds = 20;

    public InwinningRoadSegmentsMissingFromUpload(IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
        : base(ProblemCode.Upload.InwinningRoadSegmentsMissing,
            new ProblemParameter(ParameterName.RoadSegmentIds, Describe(roadSegmentIds)))
    {
    }

    private static string Describe(IReadOnlyCollection<RoadSegmentId> roadSegmentIds)
    {
        var reported = string.Join(", ", roadSegmentIds
            .Take(MaximumReportedRoadSegmentIds)
            .Select(x => x.ToString()));

        var remaining = roadSegmentIds.Count - MaximumReportedRoadSegmentIds;

        return remaining > 0
            ? $"{reported} en {remaining} andere"
            : reported;
    }
}
