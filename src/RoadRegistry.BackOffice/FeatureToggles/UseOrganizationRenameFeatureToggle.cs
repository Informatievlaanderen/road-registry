namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseOrganizationRenameFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
