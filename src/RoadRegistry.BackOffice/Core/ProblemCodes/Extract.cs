namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class Extract
    {
        public static readonly ProblemCode NotFound = new("ExtractNotFound");
        public static readonly ProblemCode GeometryInvalid = new("ExtractContourInvalid");
        public static readonly ProblemCode DescriptionIsRequired = new("ExtractDescriptionIsRequired");
        public static readonly ProblemCode DescriptionTooLong = new("ExtractDescriptionTooLong");
        public static readonly ProblemCode ExternalRequestIdInvalid = new("ExtractExternalRequestIdInvalid");
    }
}
