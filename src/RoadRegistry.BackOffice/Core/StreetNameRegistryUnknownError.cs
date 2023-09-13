namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class StreetNameRegistryUnknownError : Error
{
    public StreetNameRegistryUnknownError()
        : base(ProblemCode.StreetName.RegistryUnknownError)
    {
    }
}
