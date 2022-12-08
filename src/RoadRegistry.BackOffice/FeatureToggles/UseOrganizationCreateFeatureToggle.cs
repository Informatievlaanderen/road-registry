namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseOrganizationCreateFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
