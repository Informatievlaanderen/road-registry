namespace RoadRegistry.BackOffice.FeatureToggles;
public sealed record UseExtractZipArchiveWriterV2FeatureToggle(bool FeatureEnabled) : ApplicationFeatureToggle(FeatureEnabled);
