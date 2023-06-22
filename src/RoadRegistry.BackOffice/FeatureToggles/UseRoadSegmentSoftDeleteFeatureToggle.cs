namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseRoadSegmentSoftDeleteFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
