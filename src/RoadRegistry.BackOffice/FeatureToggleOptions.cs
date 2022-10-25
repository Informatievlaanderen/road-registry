namespace RoadRegistry.BackOffice;

using FeatureToggle;

public class FeatureToggleOptions
{
    public const string ConfigurationKey = "FeatureToggles";
    public bool UseSnapshotRebuild { get; private set; }
    public bool UseFeatureCompare { get; private set; }
    public bool UseApiKeyAuthentication { get; private set; }
}

public sealed record UseSnapshotRebuildFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);

public sealed record UseFeatureCompareFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);

public sealed record UseApiKeyAuthenticationFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);

public abstract record ApplicationFeatureToggle(bool FeatureEnabled) : IFeatureToggle
{
}