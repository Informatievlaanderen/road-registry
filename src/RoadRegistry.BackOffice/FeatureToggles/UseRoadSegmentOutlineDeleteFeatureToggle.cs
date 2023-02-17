namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseRoadSegmentOutlineDeleteFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
