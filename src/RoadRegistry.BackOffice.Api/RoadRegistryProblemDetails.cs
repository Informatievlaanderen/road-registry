namespace RoadRegistry.BackOffice.Api
{
    public static class RoadRegistryProblemDetails
    {
        public const string NamespaceTemplate = "urn:road-registry:{0}";

        public static string Format(string value) => string.Format(NamespaceTemplate, value);
    }
}
