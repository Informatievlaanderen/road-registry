namespace RoadRegistry.BackOffice.Core;

using ProblemCodes;

public class JsonInvalid : Error
{
    public JsonInvalid()
        : base(ProblemCode.Common.JsonInvalid)
    {
    }
}
