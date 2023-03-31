namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class StreetNameNotFound : Error
{
    public StreetNameNotFound()
        : base(ProblemCode.StreetName.NotFound)
    {
    }
}
