namespace RoadRegistry.BackOffice.Abstractions.Validation
{
    using System.Globalization;

    public static partial class ValidationErrors
    {
        public static class Common
        {
            public static class IncorrectObjectId
            {
                public const string Code = "IncorrectObjectId";
                public static string Message(object? value) => string.Format(CultureInfo.InvariantCulture, "De waarde '{0}' is ongeldig.", value);
            }
        }
    }
}
