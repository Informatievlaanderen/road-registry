namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseKafkaStreetNameCacheFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
