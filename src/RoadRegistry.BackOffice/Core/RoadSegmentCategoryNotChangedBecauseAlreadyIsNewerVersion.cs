namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentCategoryNotChangedBecauseAlreadyIsNewerVersion : Warning
{
    public RoadSegmentCategoryNotChangedBecauseAlreadyIsNewerVersion(RoadSegmentId segmentId)
        : base(ProblemCode.RoadSegment.Category.NotChangedBecauseAlreadyIsNewerVersion,
            new ProblemParameter("Identifier", segmentId.ToInt32().ToString()))
    {
    }
}
