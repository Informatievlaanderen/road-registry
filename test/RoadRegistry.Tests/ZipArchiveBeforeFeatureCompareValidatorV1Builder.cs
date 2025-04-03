namespace RoadRegistry.Tests
{
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.FeatureCompare;
    using RoadRegistry.BackOffice.FeatureCompare.V1.Readers;
    using RoadRegistry.BackOffice.FeatureCompare.V1.Translators;
    using RoadRegistry.BackOffice.FeatureCompare.V1.Validation;

    public static class ZipArchiveBeforeFeatureCompareValidatorV1Builder
    {
        private static readonly FileEncoding Encoding = FileEncoding.UTF8;

        public static IZipArchiveBeforeFeatureCompareValidator Create(
            IRoadSegmentFeatureCompareStreetNameContextFactory streetNameContextFactory = null) =>
            new ZipArchiveBeforeFeatureCompareValidator(
                new TransactionZoneZipArchiveValidator(new TransactionZoneFeatureCompareFeatureReader(Encoding)),
                new RoadNodeZipArchiveValidator(new RoadNodeFeatureCompareFeatureReader(Encoding)),
                new RoadSegmentZipArchiveValidator(
                    new RoadSegmentFeatureCompareFeatureReader(Encoding),
                    streetNameContextFactory ?? new FakeRoadSegmentFeatureCompareStreetNameContextFactoryV1()),
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
