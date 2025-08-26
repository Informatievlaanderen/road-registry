namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseExtractsV2FeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
