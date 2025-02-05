namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice
{
    using System.IO.Compression;
    using System.Text;
    using Abstractions;
    using AutoFixture;
    using ExtractHost;
    using Extracts;
    using FluentAssertions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.IO;
    using Moq;
    using NetTopologySuite.Geometries;
    using RoadRegistry.Tests.BackOffice;
    using RoadRegistry.Tests.BackOffice.Scenarios;

    public class RoadNetworkExtractToZipArchiveWriterTests
    {
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;
        private readonly ZipArchiveWriterOptions _zipArchiveWriterOptions;
        private readonly IStreetNameCache _streetNameCache;
        private readonly Mock<IZipArchiveDataProvider> _zipArchiveDataProvider;

        public RoadNetworkExtractToZipArchiveWriterTests(
            ZipArchiveWriterOptions zipArchiveWriterOptions,
            IStreetNameCache streetNameCache)
        {
            _memoryStreamManager = new RecyclableMemoryStreamManager();
            _zipArchiveWriterOptions = zipArchiveWriterOptions;
            _streetNameCache = streetNameCache;

            _zipArchiveDataProvider = new Mock<IZipArchiveDataProvider>();
            _zipArchiveDataProvider
                .Setup(x => x.GetOrganizations(It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            _zipArchiveDataProvider
                .Setup(x => x.GetRoadNodes(It.IsAny<IPolygonal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            _zipArchiveDataProvider
                .Setup(x => x.GetRoadSegments(It.IsAny<IPolygonal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            _zipArchiveDataProvider
                .Setup(x => x.GetRoadSegmentLaneAttributes(It.IsAny<IPolygonal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            _zipArchiveDataProvider
                .Setup(x => x.GetRoadSegmentWidthAttributes(It.IsAny<IPolygonal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            _zipArchiveDataProvider
                .Setup(x => x.GetRoadSegmentSurfaceAttributes(It.IsAny<IPolygonal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            _zipArchiveDataProvider
                .Setup(x => x.GetRoadSegmentNationalRoadAttributes(It.IsAny<IPolygonal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            _zipArchiveDataProvider
                .Setup(x => x.GetRoadSegmentEuropeanRoadAttributes(It.IsAny<IPolygonal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            _zipArchiveDataProvider
                .Setup(x => x.GetRoadSegmentNumberedRoadAttributes(It.IsAny<IPolygonal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
            _zipArchiveDataProvider
                .Setup(x => x.GetGradeSeparatedJunctions(It.IsAny<IPolygonal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
        }

        [Fact]
        public async Task InformativeExtractRequest_ThenOnlyContainsExtractAndIntegrationFiles()
        {
            var fixture = new RoadNetworkTestData().ObjectProvider;
            fixture.CustomizeNtsPolygon();

            var zipArchiveWriter = new RoadNetworkExtractToZipArchiveWriter(
                _zipArchiveWriterOptions,
                _streetNameCache,
                _memoryStreamManager,
                Encoding.UTF8,
                NullLogger<RoadNetworkExtractToZipArchiveWriter>.Instance);

            var stream = _memoryStreamManager.GetStream();

            using var archive = new ZipArchive(stream, ZipArchiveMode.Update, true, Encoding.UTF8);
            var request = new RoadNetworkExtractAssemblyRequest(
                    fixture.Create<ExternalExtractRequestId>(),
                    fixture.Create<DownloadId>(),
                    fixture.Create<ExtractDescription>(),
                    fixture.Create<IPolygonal>(),
                    isInformative: true);
            await zipArchiveWriter.WriteAsync(archive, request, _zipArchiveDataProvider.Object, CancellationToken.None);

           var fileNames =  archive.Entries.Select(x => x.FullName).ToList();

           var expectedFileNames = new[] {
               "Transactiezones.dbf",
               "Transactiezones.shp",
               "Transactiezones.shx",
               "Transactiezones.cpg",
               "Transactiezones.prj",
               "eLstOrg.dbf",
               "eWegknoop.dbf",
               "eWegknoop.shp",
               "eWegknoop.shx",
               "eWegknoop.cpg",
               "eWegknoop.prj",
               "eWegsegment.dbf",
               "eWegsegment.shp",
               "eWegsegment.shx",
               "eWegsegment.cpg",
               "eWegsegment.prj",
               "eAttRijstroken.dbf",
               "eAttWegbreedte.dbf",
               "eAttWegverharding.dbf",
               "eAttNationweg.dbf",
               "eAttEuropweg.dbf",
               "eAttGenumweg.dbf",
               "eRltOgkruising.dbf",
               "iWegsegment.dbf",
               "iWegsegment.shp",
               "iWegsegment.shx",
               "iWegsegment.cpg",
               "iWegsegment.prj",
               "iWegknoop.dbf",
               "iWegknoop.shp",
               "iWegknoop.shx",
               "iWegknoop.cpg",
               "iWegknoop.prj",
               "eWegknoopLktType.dbf",
               "eWegverhardLktType.dbf",
               "eGenumwegLktRichting.dbf",
               "eWegsegmentLktWegcat.dbf",
               "eWegsegmentLktTgbep.dbf",
               "eWegsegmentLktMethode.dbf",
               "eWegsegmentLktMorf.dbf",
               "eWegsegmentLktStatus.dbf",
               "eOgkruisingLktType.dbf",
               "eRijstrokenLktRichting.dbf"
           };

           fileNames.Should().HaveCount(expectedFileNames.Length);
           foreach (var fileName in expectedFileNames)
           {
               fileNames.Should().Contain(fileName);
           }
        }

        [Fact]
        public async Task NotInformativeExtractRequest_ThenAlsoContainsChangeFiles()
        {
            var fixture = new RoadNetworkTestData().ObjectProvider;
            fixture.CustomizeNtsPolygon();

            var zipArchiveWriter = new RoadNetworkExtractToZipArchiveWriter(
                _zipArchiveWriterOptions,
                _streetNameCache,
                _memoryStreamManager,
                Encoding.UTF8,
                NullLogger<RoadNetworkExtractToZipArchiveWriter>.Instance);

            var stream = _memoryStreamManager.GetStream();

            using var archive = new ZipArchive(stream, ZipArchiveMode.Update, true, Encoding.UTF8);
            var request = new RoadNetworkExtractAssemblyRequest(
                    fixture.Create<ExternalExtractRequestId>(),
                    fixture.Create<DownloadId>(),
                    fixture.Create<ExtractDescription>(),
                    fixture.Create<IPolygonal>(),
                    isInformative: false);
            await zipArchiveWriter.WriteAsync(archive, request, _zipArchiveDataProvider.Object, CancellationToken.None);

           var fileNames =  archive.Entries.Select(x => x.FullName).ToList();

           var expectedFileNames = new[] {
               "Transactiezones.dbf",
               "Transactiezones.shp",
               "Transactiezones.shx",
               "Transactiezones.cpg",
               "Transactiezones.prj",
               "eLstOrg.dbf",
               "eWegknoop.dbf",
               "eWegknoop.shp",
               "eWegknoop.shx",
               "eWegknoop.cpg",
               "eWegknoop.prj",
               "Wegknoop.dbf",
               "Wegknoop.shp",
               "Wegknoop.shx",
               "Wegknoop.cpg",
               "Wegknoop.prj",
               "eWegsegment.dbf",
               "eWegsegment.shp",
               "eWegsegment.shx",
               "eWegsegment.cpg",
               "eWegsegment.prj",
               "Wegsegment.dbf",
               "Wegsegment.shp",
               "Wegsegment.shx",
               "Wegsegment.cpg",
               "Wegsegment.prj",
               "eAttRijstroken.dbf",
               "AttRijstroken.dbf",
               "eAttWegbreedte.dbf",
               "AttWegbreedte.dbf",
               "eAttWegverharding.dbf",
               "AttWegverharding.dbf",
               "eAttNationweg.dbf",
               "AttNationweg.dbf",
               "eAttEuropweg.dbf",
               "AttEuropweg.dbf",
               "eAttGenumweg.dbf",
               "AttGenumweg.dbf",
               "eRltOgkruising.dbf",
               "RltOgkruising.dbf",
               "iWegsegment.dbf",
               "iWegsegment.shp",
               "iWegsegment.shx",
               "iWegsegment.cpg",
               "iWegsegment.prj",
               "iWegknoop.dbf",
               "iWegknoop.shp",
               "iWegknoop.shx",
               "iWegknoop.cpg",
               "iWegknoop.prj",
               "eWegknoopLktType.dbf",
               "eWegverhardLktType.dbf",
               "eGenumwegLktRichting.dbf",
               "eWegsegmentLktWegcat.dbf",
               "eWegsegmentLktTgbep.dbf",
               "eWegsegmentLktMethode.dbf",
               "eWegsegmentLktMorf.dbf",
               "eWegsegmentLktStatus.dbf",
               "eOgkruisingLktType.dbf",
               "eRijstrokenLktRichting.dbf"
           };

           fileNames.Should().HaveCount(expectedFileNames.Length);
           foreach (var fileName in expectedFileNames)
           {
               fileNames.Should().Contain(fileName);
           }
        }
    }
}
