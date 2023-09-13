namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseNetTopologySuiteShapeReaderWriterFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
