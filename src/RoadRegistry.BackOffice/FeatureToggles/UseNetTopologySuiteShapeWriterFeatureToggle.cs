namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseNetTopologySuiteShapeWriterFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
