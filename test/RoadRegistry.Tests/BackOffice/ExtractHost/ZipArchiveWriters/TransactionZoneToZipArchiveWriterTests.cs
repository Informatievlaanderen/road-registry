namespace RoadRegistry.BackOffice.ExtractHost.ZipArchiveWriters
{
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Editor.Schema;
    using Extracts;
    using FluentAssertions;
    using Messages;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;
    using NetTopologySuite.Geometries;
    using RoadRegistry.Framework.Projections;
    using Xunit;
    using TransactionZoneDbaseRecord = Editor.Schema.Extracts.TransactionZoneDbaseRecord;

    public class TransactionZoneToZipArchiveWriterTests
    {
        private readonly TransactionZoneToZipArchiveWriter _sut;
        private readonly Fixture _fixture;


        public TransactionZoneToZipArchiveWriterTests()
        {
            _sut = new TransactionZoneToZipArchiveWriter(Encoding.UTF8);
            _fixture = new Fixture();

            CustomizeRoadNetworkExtractAssemblyRequestFixture(_fixture);
        }

        [Fact]
        public Task WriteAsyncWritesExpectedDownloadId()
        {
            var editorContext = CreateContextFor(nameof(WriteAsyncWritesExpectedDownloadId));
            var request = _fixture.Create<RoadNetworkExtractAssemblyRequest>();

            return new ZipArchiveScenarioWithWriter<EditorContext>(new RecyclableMemoryStreamManager(), _sut)
                .WithContext(editorContext)
                .WithRequest(request)
                .Assert(readArchive =>
                {
                    var records = ReadFromArchive<TransactionZoneDbaseRecord>(readArchive, "Transactiezones.dbf").ToList();

                    records.Count.Should().Be(1);
                    records[0].Item2.DOWNLOADID.Value.Should().Be(request.DownloadId.ToGuid().ToString("N"));
                });
        }

        [Theory]
        [MemberData(nameof(WriteAsyncWritesExpectedBeschrijvCases))]
        public Task WriteAsyncWritesExpectedBeschrijv(ExtractDescription extractDescription, ExternalExtractRequestId externalRequestId, string expectedBeschrijv)
        {
            var editorContext = CreateContextFor(nameof(WriteAsyncWritesExpectedDownloadId));
            var request = CreateRoadNetworkExtractAssemblyRequest(extractDescription, externalRequestId);

            return new ZipArchiveScenarioWithWriter<EditorContext>(new RecyclableMemoryStreamManager(), _sut)
                .WithContext(editorContext)
                .WithRequest(request)
                .Assert(readArchive =>
                {
                    var records = ReadFromArchive<TransactionZoneDbaseRecord>(readArchive, "Transactiezones.dbf").ToList();

                    records.Count.Should().Be(1);
                    records[0].Item2.BESCHRIJV.Value.Should().Be(expectedBeschrijv);
                });
        }

        public static IEnumerable<object[]> WriteAsyncWritesExpectedBeschrijvCases()
        {
            yield return new object[]
            {
                new ExtractDescription("description"),
                new ExternalExtractRequestId("external request id"),
                "description"
            };
            yield return new object[]
            {
                new ExtractDescription(string.Empty),
                new ExternalExtractRequestId("external request id"),
                "external request id"
            };
            // // Empty external extract request id not allowed
            //yield return new object[]
            //{
            //    new ExtractDescription("description"),
            //    new ExternalExtractRequestId(""),
            //    "description"
            //};
        }

        private RoadNetworkExtractAssemblyRequest CreateRoadNetworkExtractAssemblyRequest(ExtractDescription extractDescription, ExternalExtractRequestId externalRequestId)
        {
            var fixture = _fixture.Create<RoadNetworkExtractAssemblyRequest>();

            return new RoadNetworkExtractAssemblyRequest(
                externalRequestId,
                fixture.DownloadId,
                extractDescription,
                fixture.Contour);
        }

        private static IEnumerable<(RecordNumber, TDbaseRecord)> ReadFromArchive<TDbaseRecord>(ZipArchive archive, string fileName) where TDbaseRecord : DbaseRecord, new()
        {
            var archiveFileEntry = archive.GetEntry(fileName);
            Assert.NotNull(archiveFileEntry);

            using (var entryStream = archiveFileEntry.Open())
            using (var reader = new BinaryReader(entryStream, Encoding.UTF8))
            {
                var header = DbaseFileHeader.Read(reader, new DbaseFileHeaderReadBehavior(true));
                var records = header.CreateDbaseRecordEnumerator<TDbaseRecord>(reader);

                while (records.MoveNext())
                {
                    yield return (records.CurrentRecordNumber, records.Current);
                }
            }
        }

        private static EditorContext CreateContextFor(string database)
        {
            var options = new DbContextOptionsBuilder<EditorContext>()
                .UseInMemoryDatabase(database)
                .EnableSensitiveDataLogging()
                .Options;

            return new MemoryEditorContext(options);
        }

        private static void CustomizeRoadNetworkExtractAssemblyRequestFixture(IFixture fixture)
        {
            fixture.CustomizeExternalExtractRequestId();
            fixture.CustomizeDownloadId();
            fixture.CustomizeExtractDescription();
            fixture.Register(() => MultiPolygon.Empty);
            fixture.CustomizeRoadNetworkExtractGeometry();

            var geometry = fixture.Create<RoadNetworkExtractGeometry>();

            fixture.Customize<RoadNetworkExtractAssemblyRequest>(customization =>
                customization.FromFactory(composer => new RoadNetworkExtractAssemblyRequest(
                    fixture.Create<ExternalExtractRequestId>(),
                    fixture.Create<DownloadId>(),
                    fixture.Create<ExtractDescription>(),
                    GeometryTranslator.Translate(geometry))));
        }
    }
}
