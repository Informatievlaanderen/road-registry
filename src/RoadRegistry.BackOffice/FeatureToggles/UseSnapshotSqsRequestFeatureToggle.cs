namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseSnapshotSqsRequestFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
