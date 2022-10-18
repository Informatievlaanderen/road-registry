namespace RoadRegistry.BackOffice.Api.Infrastructure.Configuration;

public sealed class FeatureToggleOptions
{
    public const string ConfigurationKey = "FeatureToggles";
    public bool UseApiKeyAuthentication { get; set; }
    public bool UseFeatureCompare { get; set; }
}