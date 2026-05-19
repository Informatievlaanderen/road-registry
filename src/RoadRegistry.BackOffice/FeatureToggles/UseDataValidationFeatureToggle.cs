namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseDataValidationFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
