namespace RoadRegistry.Tests
{
    using Extracts;
    using Extracts.FeatureCompare.V3;
    using Extracts.FeatureCompare.V3.EuropeanRoad;
    using Extracts.FeatureCompare.V3.GradeSeparatedJunction;
    using Extracts.FeatureCompare.V3.NationalRoad;
    using Extracts.FeatureCompare.V3.RoadNode;
    using Extracts.FeatureCompare.V3.RoadSegment;
    using Extracts.FeatureCompare.V3.RoadSegmentSurface;
    using Extracts.FeatureCompare.V3.TransactionZone;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using RoadRegistry.Infrastructure;
    using IZipArchiveFeatureCompareTranslator = Extracts.FeatureCompare.V3.IZipArchiveFeatureCompareTranslator;

    public static class ZipArchiveFeatureCompareTranslatorV3Builder
    {
        private static readonly FileEncoding Encoding = FileEncoding.UTF8;

        public static IZipArchiveFeatureCompareTranslator Create(
            IOrganizationCache organizationCache = null,
            IRoadSegmentFeatureCompareStreetNameContextFactory streetNameContextFactory = null,
            ILoggerFactory loggerFactory = null)
        {
            return new ZipArchiveFeatureCompareTranslator(
                new TransactionZoneFeatureCompareTranslator(new TransactionZoneFeatureCompareFeatureReader(Encoding)),
                new RoadNodeFeatureCompareTranslator(new RoadNodeFeatureCompareFeatureReader(Encoding)),
                new RoadSegmentFeatureCompareTranslator(
                    new RoadSegmentFeatureCompareFeatureReader(Encoding),
                    streetNameContextFactory ?? new FakeRoadSegmentFeatureCompareStreetNameContextFactoryV3(),
                    organizationCache ?? new FakeOrganizationCache()
                ),
                new RoadSegmentSurfaceFeatureCompareTranslator(new RoadSegmentSurfaceFeatureCompareFeatureReader(Encoding)),
                new EuropeanRoadFeatureCompareTranslator(new EuropeanRoadFeatureCompareFeatureReader(Encoding)),
                new NationalRoadFeatureCompareTranslator(new NationalRoadFeatureCompareFeatureReader(Encoding)),
                new GradeSeparatedJunctionFeatureCompareTranslator(new GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding)),
                loggerFactory ?? new NullLoggerFactory()
            );
        }
    }
}
