namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentCategoryNotChangedBecauseCurrentIsNewerVersion : Warning
{
    public RoadSegmentCategoryNotChangedBecauseCurrentIsNewerVersion(RoadSegmentId segmentId)
        : base(ProblemCode.RoadSegment.Category.NotChangedBecauseCurrentIsNewerVersion,
            new ProblemParameter("Identifier", segmentId.ToInt32().ToString()))
    {
    }
}
