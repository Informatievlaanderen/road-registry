namespace RoadRegistry.BackOffice.Abstractions.Validation;

public static partial class ValidationErrors
{
    public static class RoadNetwork
    {
        public static class NotFound
        {
            public const string Code = Common.NotFound.Code;
            public const string Message = "Onbestaand wegen netwerk.";
        }
    }
}
