namespace RoadRegistry.BackOffice.Abstractions.Validation
{
    public static partial class ValidationErrors
    {
        public static class StreetName
        {
            public static class NotFound
            {
                public const string Code = "StraatnaamNietGekendValidatie";
                public const string Message = "De straatnaam is niet gekend in het Straatnamenregister.";
            }
        }
    }
}
