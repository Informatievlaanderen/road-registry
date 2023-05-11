namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseZipArchiveFeatureCompareTranslatorFeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
