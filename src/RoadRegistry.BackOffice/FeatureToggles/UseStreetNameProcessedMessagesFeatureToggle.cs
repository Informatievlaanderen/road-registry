namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseStreetNameProcessedMessagesFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
