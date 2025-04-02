namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseNetTopologySuiteForExtractFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
