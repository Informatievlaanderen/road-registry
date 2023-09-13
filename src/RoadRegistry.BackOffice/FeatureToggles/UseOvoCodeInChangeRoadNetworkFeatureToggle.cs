namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseOvoCodeInChangeRoadNetworkFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
