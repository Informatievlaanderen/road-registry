namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseRoadSegmentChangeAttributesFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
