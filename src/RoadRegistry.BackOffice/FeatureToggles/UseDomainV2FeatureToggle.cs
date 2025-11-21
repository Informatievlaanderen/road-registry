namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseDomainV2FeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
