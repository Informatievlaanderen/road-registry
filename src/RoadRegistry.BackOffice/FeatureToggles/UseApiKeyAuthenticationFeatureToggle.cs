namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseApiKeyAuthenticationFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
