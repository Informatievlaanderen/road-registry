namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseRoadSegmentV2EventProcessorFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
