namespace RoadRegistry.ValueObjects.Problems;

using ProblemCodes;

public class StreetNameRegistryUnexpectedError : Error
{
    public StreetNameRegistryUnexpectedError(int statusCode)
        : base(ProblemCode.StreetName.RegistryUnexpectedError,
            new ProblemParameter("StatusCode", statusCode.ToString())
        )
    {
    }
}
