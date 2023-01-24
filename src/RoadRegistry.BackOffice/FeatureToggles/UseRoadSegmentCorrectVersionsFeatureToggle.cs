namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseRoadSegmentCorrectVersionsFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
