namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseSnapshotRebuildFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
