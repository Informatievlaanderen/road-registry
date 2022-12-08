namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseOrganizationDeleteFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
