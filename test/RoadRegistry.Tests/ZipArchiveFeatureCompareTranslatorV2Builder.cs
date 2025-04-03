namespace RoadRegistry.Tests
{
    using Microsoft.Extensions.Logging.Abstractions;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.FeatureCompare;
    using RoadRegistry.BackOffice.FeatureCompare.V2;
    using RoadRegistry.BackOffice.FeatureCompare.V2.Readers;
    using RoadRegistry.BackOffice.FeatureCompare.V2.Translators;

    public static class ZipArchiveFeatureCompareTranslatorV2Builder
    {
        private static readonly FileEncoding Encoding = FileEncoding.UTF8;

        public static IZipArchiveFeatureCompareTranslator Create(
            IOrganizationCache organizationCache = null,
            IRoadSegmentFeatureCompareStreetNameContextFactory streetNameContextFactory = null)
        {
            return new ZipArchiveFeatureCompareTranslator(
                new TransactionZoneFeatureCompareTranslator(new TransactionZoneFeatureCompareFeatureReader(Encoding)),
                new RoadNodeFeatureCompareTranslator(new RoadNodeFeatureCompareFeatureReader(Encoding)),
                new RoadSegmentFeatureCompareTranslator(
                    new RoadSegmentFeatureCompareFeatureReader(Encoding),
                    streetNameContextFactory ?? new FakeRoadSegmentFeatureCompareStreetNameContextFactoryV2(),
                    organizationCache ?? new FakeOrganizationCache()
                ),
                new RoadSegmentLaneFeatureCompareTranslator(new RoadSegmentLaneFeatureCompareFeatureReader(Encoding)),
                new RoadSegmentWidthFeatureCompareTranslator(new RoadSegmentWidthFeatureCompareFeatureReader(Encoding)),
                new RoadSegmentSurfaceFeatureCompareTranslator(new RoadSegmentSurfaceFeatureCompareFeatureReader(Encoding)),
                new EuropeanRoadFeatureCompareTranslator(new EuropeanRoadFeatureCompareFeatureReader(Encoding)),
                new NationalRoadFeatureCompareTranslator(new NationalRoadFeatureCompareFeatureReader(Encoding)),
                new NumberedRoadFeatureCompareTranslator(new NumberedRoadFeatureCompareFeatureReader(Encoding)),
                new GradeSeparatedJunctionFeatureCompareTranslator(new GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding)),
                new NullLoggerFactory()
            );
        }
    }
}
