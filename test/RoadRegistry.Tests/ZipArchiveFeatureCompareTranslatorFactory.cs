namespace RoadRegistry.Tests
{
    using Microsoft.Extensions.Logging;
    using RoadRegistry.BackOffice;
    using RoadRegistry.BackOffice.FeatureCompare;
    using RoadRegistry.BackOffice.FeatureCompare.Translators;
    using RoadRegistry.BackOffice.FeatureToggles;

    public static class ZipArchiveFeatureCompareTranslatorFactory
    {
        private static readonly FileEncoding Encoding = FileEncoding.UTF8;

        public static IZipArchiveFeatureCompareTranslator Create(ILogger<ZipArchiveFeatureCompareTranslator> logger) => new ZipArchiveFeatureCompareTranslator(
            new TransactionZoneFeatureCompareTranslator(new TransactionZoneFeatureCompareFeatureReader(Encoding)),
            new RoadNodeFeatureCompareTranslator(new RoadNodeFeatureCompareFeatureReader(Encoding)),
            new RoadSegmentFeatureCompareTranslator(new RoadSegmentFeatureCompareFeatureReader(Encoding), new FakeOrganizationRepository()),
            new RoadSegmentLaneFeatureCompareTranslator(new RoadSegmentLaneFeatureCompareFeatureReader(Encoding)),
            new RoadSegmentWidthFeatureCompareTranslator(new RoadSegmentWidthFeatureCompareFeatureReader(Encoding)),
            new RoadSegmentSurfaceFeatureCompareTranslator(new RoadSegmentSurfaceFeatureCompareFeatureReader(Encoding)),
            new EuropeanRoadFeatureCompareTranslator(new EuropeanRoadFeatureCompareFeatureReader(Encoding)),
            new NationalRoadFeatureCompareTranslator(new NationalRoadFeatureCompareFeatureReader(Encoding)),
            new NumberedRoadFeatureCompareTranslator(new NumberedRoadFeatureCompareFeatureReader(Encoding)),
            new GradeSeparatedJunctionFeatureCompareTranslator(
                new GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding),
                new UseGradeSeparatedJunctionLowerRoadSegmentEqualsUpperRoadSegmentValidationFeatureToggle(true)
            ),
            logger
        );
    }
}
