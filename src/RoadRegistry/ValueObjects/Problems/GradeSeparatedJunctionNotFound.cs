namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class GradeSeparatedJunctionNotFound : Error
{
    public GradeSeparatedJunctionNotFound()
        : base(ProblemCode.GradeSeparatedJunction.NotFound)
    {
    }
}
