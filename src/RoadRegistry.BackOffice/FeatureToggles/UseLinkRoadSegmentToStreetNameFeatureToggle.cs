namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseLinkRoadSegmentToStreetNameFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
