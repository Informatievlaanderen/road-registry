namespace RoadRegistry.BackOffice.Abstractions.Validation;

public static partial class ValidationErrors
{
    public static class Organization
    {
        public static class NotFound
        {
            public const string Code = "WegbeheerderNietCorrect";

            public static string Message(string value)
            {
                return $"Wegbeheerder is foutief. '{value}' is geen geldige waarde.";
            }
        }

        public static class IsRequired
        {
            public const string Code = "WegbeheerderVerplicht";
            public const string Message = "Wegbeheerder is verplicht.";
        }
    }
}
