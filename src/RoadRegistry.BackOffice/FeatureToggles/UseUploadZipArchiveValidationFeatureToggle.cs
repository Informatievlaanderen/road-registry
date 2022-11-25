namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseUploadZipArchiveValidationFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
