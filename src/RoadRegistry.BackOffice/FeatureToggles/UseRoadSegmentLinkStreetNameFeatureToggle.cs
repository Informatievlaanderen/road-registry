namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseRoadSegmentLinkStreetNameFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
