namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseOrganizationProcessedMessagesFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
