namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentCategoryNotChanged : Warning
{
    public RoadSegmentCategoryNotChanged()
        : base(ProblemCode.RoadSegment.Category.NotChanged)
    {
    }

    public RoadSegmentCategoryNotChanged(RoadSegmentId segmentId)
        : base(ProblemCode.RoadSegment.Category.NotChanged,
            new ProblemParameter("SegmentId", segmentId.ToInt32().ToString()))
    {
    }
}
