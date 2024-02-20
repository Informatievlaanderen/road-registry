namespace RoadRegistry.Tests
{
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.FeatureCompare.Translators;
    using RoadRegistry.BackOffice.Uploads;
    using RoadRegistry.BackOffice.ZipArchiveWriters.Validation;

    public static class ZipArchiveBeforeFeatureCompareValidatorFactory
    {
        private static readonly FileEncoding Encoding = FileEncoding.UTF8;

        public static IZipArchiveBeforeFeatureCompareValidator Create(IStreetNameCache streetNameCache = null) => new ZipArchiveBeforeFeatureCompareValidator(
            new TransactionZoneFeatureCompareFeatureReader(Encoding),
            new RoadNodeFeatureCompareFeatureReader(Encoding),
            new RoadSegmentFeatureCompareFeatureReader(Encoding),
            new RoadSegmentLaneFeatureCompareFeatureReader(Encoding),
            new RoadSegmentWidthFeatureCompareFeatureReader(Encoding),
            new RoadSegmentSurfaceFeatureCompareFeatureReader(Encoding),
            new EuropeanRoadFeatureCompareFeatureReader(Encoding),
            new NationalRoadFeatureCompareFeatureReader(Encoding),
            new NumberedRoadFeatureCompareFeatureReader(Encoding),
            new GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding),
            streetNameCache ?? new FakeStreetNameCache()
        );
    }
}
