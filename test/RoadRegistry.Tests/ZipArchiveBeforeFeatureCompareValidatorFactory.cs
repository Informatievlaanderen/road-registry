namespace RoadRegistry.Tests
{
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.FeatureCompare.Readers;
    using RoadRegistry.BackOffice.FeatureCompare.Validation;
    using RoadRegistry.BackOffice.Uploads;

    public static class ZipArchiveBeforeFeatureCompareValidatorFactory
    {
        private static readonly FileEncoding Encoding = FileEncoding.UTF8;

        public static IZipArchiveBeforeFeatureCompareValidator Create(IStreetNameCache streetNameCache = null) => new ZipArchiveBeforeFeatureCompareValidator(
            new TransactionZoneZipArchiveValidator(new TransactionZoneFeatureCompareFeatureReader(Encoding)),
            new RoadNodeZipArchiveValidator(new RoadNodeFeatureCompareFeatureReader(Encoding)),
            new RoadSegmentZipArchiveValidator(new RoadSegmentFeatureCompareFeatureReader(Encoding), streetNameCache ?? new FakeStreetNameCache()),
            new RoadSegmentLaneZipArchiveValidator(new RoadSegmentLaneFeatureCompareFeatureReader(Encoding)),
            new RoadSegmentWidthZipArchiveValidator(new RoadSegmentWidthFeatureCompareFeatureReader(Encoding)),
            new RoadSegmentSurfaceZipArchiveValidator(new RoadSegmentSurfaceFeatureCompareFeatureReader(Encoding)),
            new EuropeanRoadZipArchiveValidator(new EuropeanRoadFeatureCompareFeatureReader(Encoding)),
            new NationalRoadZipArchiveValidator(new NationalRoadFeatureCompareFeatureReader(Encoding)),
            new NumberedRoadZipArchiveValidator(new NumberedRoadFeatureCompareFeatureReader(Encoding)),
            new GradeSeparatedJunctionZipArchiveValidator(new GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding))
        );
    }
}
