namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class GradeSeparatedJunctionNotFound : Error
{
    public GradeSeparatedJunctionNotFound()
        : base(ProblemCode.GradeSeparatedJunction.NotFound)
    {
    }

    public GradeSeparatedJunctionNotFound(GradeSeparatedJunctionId segmentId)
        : base(ProblemCode.GradeSeparatedJunction.NotFound,
            new ProblemParameter("Identifier", segmentId.ToInt32().ToString()))
    {
    }
}
