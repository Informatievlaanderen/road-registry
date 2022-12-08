namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseRoadSegmentUnlinkStreetNameFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
