namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.FeatureCompare.DomainV2
{
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Logging.Abstractions;
    using Moq;
    using RoadRegistry.Extracts;
    using RoadRegistry.Extracts.FeatureCompare.DomainV2;
    using RoadRegistry.Extracts.FeatureCompare.DomainV2.EuropeanRoad;
    using RoadRegistry.Extracts.FeatureCompare.DomainV2.GradeSeparatedJunction;
    using RoadRegistry.Extracts.FeatureCompare.DomainV2.NationalRoad;
    using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadNode;
    using RoadRegistry.Extracts.FeatureCompare.DomainV2.RoadSegment;
    using RoadRegistry.Extracts.FeatureCompare.DomainV2.TransactionZone;
    using RoadRegistry.Infrastructure;
    using IZipArchiveFeatureCompareTranslator = RoadRegistry.Extracts.FeatureCompare.DomainV2.IZipArchiveFeatureCompareTranslator;

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
                    organizationCache ?? new FakeOrganizationCache(),
                    Mock.Of<IGrbOgcApiFeaturesDownloader>()
                ),
                new EuropeanRoadFeatureCompareTranslator(new EuropeanRoadFeatureCompareFeatureReader(Encoding)),
                new NationalRoadFeatureCompareTranslator(new NationalRoadFeatureCompareFeatureReader(Encoding)),
                new GradeSeparatedJunctionFeatureCompareTranslator(new GradeSeparatedJunctionFeatureCompareFeatureReader(Encoding)),
                loggerFactory ?? new NullLoggerFactory()
            );
        }
    }
}
