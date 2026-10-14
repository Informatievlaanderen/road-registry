namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.Inwinning
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using RoadRegistry.Extracts;
    using RoadRegistry.Extracts.FeatureCompare.Inwinning;
    using RoadRegistry.Extracts.FeatureCompare.Inwinning.EuropeanRoad;
    using RoadRegistry.Extracts.FeatureCompare.Inwinning.GradeSeparatedJunction;
    using RoadRegistry.Extracts.FeatureCompare.Inwinning.NationalRoad;
    using RoadRegistry.Extracts.FeatureCompare.Inwinning.RoadNode;
    using RoadRegistry.Extracts.FeatureCompare.Inwinning.RoadSegment;
    using RoadRegistry.Extracts.FeatureCompare.Inwinning.TransactionZone;
    using RoadRegistry.Infrastructure;
    using IZipArchiveFeatureCompareTranslator = RoadRegistry.Extracts.FeatureCompare.Inwinning.IZipArchiveFeatureCompareTranslator;

    public static class ZipArchiveFeatureCompareTranslatorV3Builder
    {
        private static readonly FileEncoding Encoding = FileEncoding.UTF8;

        public static IZipArchiveFeatureCompareTranslator Create(
            IOrganizationCache organizationCache = null,
            IRoadSegmentFeatureCompareStreetNameContextFactory streetNameContextFactory = null,
            IGrbOgcApiFeaturesDownloader grbOgcApiFeaturesDownloader = null,
            ILoggerFactory loggerFactory = null)
        {
            return new ZipArchiveFeatureCompareTranslator(
                new TransactionZoneFeatureCompareTranslator(new TransactionZoneFeatureCompareFeatureReader(Encoding)),
                new RoadNodeFeatureCompareTranslator(new RoadNodeFeatureCompareFeatureReader(Encoding)),
                new RoadSegmentFeatureCompareTranslator(
                    new RoadSegmentFeatureCompareFeatureReader(Encoding),
                    streetNameContextFactory ?? new FakeRoadSegmentFeatureCompareStreetNameContextFactoryV3(),
                    organizationCache ?? new FakeOrganizationCache(),
                    grbOgcApiFeaturesDownloader ?? new FakeInwinningGrbOgcApiFeaturesDownloader(),
                    loggerFactory
                ),
                new EuropeanRoadFeatureCompareTranslator(new EuropeanRoadFeatureCompareFeatureReader(Encoding)),
                new NationalRoadFeatureCompareTranslator(new NationalRoadFeatureCompareFeatureReader(Encoding), loggerFactory),
                new GradeSeparatedJunctionFeatureCompareTranslator(new GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding), loggerFactory),
                loggerFactory ?? new NullLoggerFactory()
            );
        }
    }
}
