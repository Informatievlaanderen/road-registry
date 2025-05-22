namespace RoadRegistry.BackOffice.FeatureToggles;

public sealed record UseGrbExtractZipArchiveWriterV2FeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
