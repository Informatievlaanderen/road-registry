namespace RoadRegistry.BackOffice.Abstractions.Validation
{
    using System.Globalization;

    public static partial class ValidationErrors
    {
        public static class Common
        {
            public static class NotFound
            {
                public const string Code = "NotFound";
                public const string Message = "De waarde ontbreekt.";
            }

            public static class IncorrectObjectId
            {
                public const string Code = "IncorrectObjectId";
                public static string Message(object? value) => string.Format(CultureInfo.InvariantCulture, "De waarde '{0}' is ongeldig.", value);
            }

            public static class JsonInvalid
            {
                public const string Code = "JsonInvalid";
                public const string Message = "Json is not valid.";
            }
        }
    }
}
