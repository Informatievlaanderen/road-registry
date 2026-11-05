namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseEditableExtractDataNearInwinningFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
