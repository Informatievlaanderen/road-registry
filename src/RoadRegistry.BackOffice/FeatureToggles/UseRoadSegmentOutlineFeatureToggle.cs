namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseRoadSegmentOutlineFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
