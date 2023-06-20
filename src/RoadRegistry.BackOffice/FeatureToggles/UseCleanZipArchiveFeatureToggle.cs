namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseCleanZipArchiveFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
