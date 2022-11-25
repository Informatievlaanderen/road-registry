namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseFeatureCompareFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
