namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseHealthChecksFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
