namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.BackOffice.ExtractZipArchiveWriter.Inwinning
{
    using System.IO.Compression;
    using System.Text;
    using AutoFixture;
    using FluentAssertions;
    using Microsoft.Extensions.Logging.Abstractions;
    using Microsoft.IO;
    using Moq;
    using NetTopologySuite.Geometries;
    using RoadRegistry.Extracts;
    using RoadRegistry.Extracts.ZipArchiveWriters;
    using RoadRegistry.Extracts.ZipArchiveWriters.Writers.Inwinning;
    using RoadRegistry.Tests.BackOffice;
    using RoadRegistry.Tests.BackOffice.Scenarios;

    public class RoadNetworkExtractZipArchiveWriterTests
    {
        private readonly RecyclableMemoryStreamManager _memoryStreamManager;
        private readonly ZipArchiveWriterOptions _zipArchiveWriterOptions;
        private readonly Mock<IZipArchiveDataProvider> _zipArchiveDataProvider;

        public RoadNetworkExtractZipArchiveWriterTests(ZipArchiveWriterOptions zipArchiveWriterOptions)
        {
            _memoryStreamManager = new RecyclableMemoryStreamManager();
            _zipArchiveWriterOptions = zipArchiveWriterOptions;

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
                .Setup(x => x.GetGradeSeparatedJunctions(It.IsAny<IPolygonal>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);
        }

        [Fact]
        public async Task InformativeExtractRequest_ThenOnlyContainsExtractAndIntegrationFiles()
        {
            var fixture = new RoadNetworkTestData().ObjectProvider;
            fixture.CustomizeNtsPolygon(srid: WellknownSrids.Lambert08);

            var zipArchiveWriter = new RoadNetworkExtractZipArchiveWriter(
                _zipArchiveWriterOptions,
                _memoryStreamManager,
                Encoding.UTF8,
                NullLoggerFactory.Instance);

            var stream = _memoryStreamManager.GetStream();

            using var archive = new ZipArchive(stream, ZipArchiveMode.Update, true, Encoding.UTF8);
            var request = new RoadNetworkExtractAssemblyRequest(
                    fixture.Create<DownloadId>(),
                    fixture.Create<ExtractDescription>(),
                    fixture.Create<IPolygonal>(),
                    isInformative: true,
                    zipArchiveWriterVersion: WellKnownZipArchiveWriterVersions.DomainV2_Inwinning);
            await zipArchiveWriter.WriteAsync(archive, request, _zipArchiveDataProvider.Object, new ZipArchiveWriteContext(), CancellationToken.None);

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
               "eAttNationweg.dbf",
               "eAttEuropweg.dbf",
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
               "eWegsegmentLktVerharding.dbf",
               "eWegsegmentLktWegcat.dbf",
               "eWegsegmentLktTgbep.dbf",
               "eWegsegmentLktMorf.dbf",
               "eWegsegmentLktStatus.dbf",
               "eOgkruisingLktType.dbf",
           };

           foreach (var fileName in expectedFileNames)
           {
               fileNames.Should().Contain(fileName);
           }

           fileNames.Should().HaveCount(expectedFileNames.Length);
        }

        [Fact]
        public async Task NotInformativeExtractRequest_ThenAlsoContainsChangeFiles()
        {
            var fixture = new RoadNetworkTestData().ObjectProvider;
            fixture.CustomizeNtsPolygon(srid: WellknownSrids.Lambert08);

            var zipArchiveWriter = new RoadNetworkExtractZipArchiveWriter(
                _zipArchiveWriterOptions,
                _memoryStreamManager,
                Encoding.UTF8,
                NullLoggerFactory.Instance);

            var stream = _memoryStreamManager.GetStream();

            using var archive = new ZipArchive(stream, ZipArchiveMode.Update, true, Encoding.UTF8);
            var request = new RoadNetworkExtractAssemblyRequest(
                    fixture.Create<DownloadId>(),
                    fixture.Create<ExtractDescription>(),
                    fixture.Create<IPolygonal>(),
                    isInformative: false,
                    zipArchiveWriterVersion: WellKnownZipArchiveWriterVersions.DomainV2_Inwinning);
            await zipArchiveWriter.WriteAsync(archive, request, _zipArchiveDataProvider.Object, new ZipArchiveWriteContext(), CancellationToken.None);

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
               "eAttNationweg.dbf",
               "AttNationweg.dbf",
               "eAttEuropweg.dbf",
               "AttEuropweg.dbf",
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
               "eWegsegmentLktVerharding.dbf",
               "eWegsegmentLktWegcat.dbf",
               "eWegsegmentLktTgbep.dbf",
               "eWegsegmentLktMorf.dbf",
               "eWegsegmentLktStatus.dbf",
               "eOgkruisingLktType.dbf",
           };

           foreach (var fileName in expectedFileNames)
           {
               fileNames.Should().Contain(fileName);
           }

           fileNames.Should().HaveCount(expectedFileNames.Length);
        }
    }
}
