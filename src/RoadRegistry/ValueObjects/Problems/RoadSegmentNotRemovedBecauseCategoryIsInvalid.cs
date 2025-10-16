namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class RoadSegmentNotRemovedBecauseCategoryIsInvalid : Error
{
    public RoadSegmentNotRemovedBecauseCategoryIsInvalid(RoadSegmentId id, RoadSegmentCategory category)
        : base(ProblemCode.RoadSegment.NotRemovedBecauseCategoryIsInvalid,
            new ProblemParameter("Identifier", id.ToString()),
            new ProblemParameter("Category", category.Translation.Name))
    {
    }
}
