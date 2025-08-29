namespace RoadRegistry.BackOffice.Core.ProblemCodes;

public sealed partial record ProblemCode
{
    public static class Extract
    {
        public static readonly ProblemCode NotFound = new("ExtractNotFound");
        public static readonly ProblemCode ContourInvalid = new("ExtractContourInvalid");
        public static readonly ProblemCode BeschrijvingIsRequired = new("ExtractDescriptionIsRequired");
        public static readonly ProblemCode BeschrijvingTooLong = new("ExtractDescriptionTooLong");
        public static readonly ProblemCode ExterneIdInvalid = new("ExtractExternalRequestIdInvalid");
    }
}
