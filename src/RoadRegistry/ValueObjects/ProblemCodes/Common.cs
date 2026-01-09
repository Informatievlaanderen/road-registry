namespace RoadRegistry.ValueObjects.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class Common
    {
        public static readonly ProblemCode NotFound = new("NotFound");
        public static readonly ProblemCode IncorrectObjectId = new("IncorrectObjectId");
        public static readonly ProblemCode IsRequired = new("IsRequired");
        public static readonly ProblemCode JsonInvalid = new("JsonInvalid");
    }
}
