namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;
using RoadRegistry.RoadSegment.ValueObjects;

public class RoadSegmentNotRemovedBecauseCategoryIsInvalid : Error
{
    public RoadSegmentNotRemovedBecauseCategoryIsInvalid(RoadSegmentId id, RoadSegmentCategory category)
        : base(ProblemCode.RoadSegment.NotRemovedBecauseCategoryIsInvalid,
            new ProblemParameter("Identifier", id.ToString()),
            new ProblemParameter("Category", category.Translation.Name))
    {
    }

    public RoadSegmentNotRemovedBecauseCategoryIsInvalid(RoadSegmentId id, RoadSegmentCategoryV2 category)
        : base(ProblemCode.RoadSegment.NotRemovedBecauseCategoryIsInvalid,
            new ProblemParameter("Identifier", id.ToString()),
            new ProblemParameter("Category", category.Translation.Name))
    {
    }
}
