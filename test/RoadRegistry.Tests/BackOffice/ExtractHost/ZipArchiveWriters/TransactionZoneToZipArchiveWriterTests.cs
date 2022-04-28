namespace RoadRegistry.BackOffice.ExtractHost.ZipArchiveWriters
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;
    using System.Threading;
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
            fixture.Register(() => MultiPolygon.Empty);
            fixture.CustomizeRoadNetworkExtractGeometry();

            var geometry = fixture.Create<RoadNetworkExtractGeometry>();

            fixture.Customize<RoadNetworkExtractAssemblyRequest>(customization =>
                customization.FromFactory(composer => new RoadNetworkExtractAssemblyRequest(
                    fixture.Create<ExternalExtractRequestId>(),
                    fixture.Create<DownloadId>(),
                    GeometryTranslator.Translate(geometry))));
        }
    }

    public abstract class ZipArchiveScenarioAbstr
    {
        private readonly RecyclableMemoryStreamManager _manager;

        protected ZipArchiveScenarioAbstr(RecyclableMemoryStreamManager manager)
        {
            _manager = manager;
        }

        protected abstract Task Write(ZipArchive archive, CancellationToken ct);

        public async Task Assert(Action<ZipArchive> assert)
        {
            using (var memoryStream = _manager.GetStream())
            {
                using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true, Encoding.UTF8))
                {
                    await Write(archive, CancellationToken.None);
                }
                memoryStream.Position = 0;
                using (var readArchive = new ZipArchive(memoryStream, ZipArchiveMode.Read, false, Encoding.UTF8))
                {
                    assert(readArchive);
                }
            }
        }

    }

    public class ZipArchiveScenarioWithWriter<TContext> : ZipArchiveScenarioAbstr where TContext : DbContext
    {
        private readonly IZipArchiveWriter<TContext> _writer;
        private TContext _context;
        private RoadNetworkExtractAssemblyRequest _request;

        public ZipArchiveScenarioWithWriter(RecyclableMemoryStreamManager manager, IZipArchiveWriter<TContext> writer) : base (manager)
        {
            _writer = writer;
        }

        public ZipArchiveScenarioWithWriter<TContext> WithContext(TContext context)
        {
            _context = context;
            return this;
        }

        public ZipArchiveScenarioWithWriter<TContext> WithRequest(RoadNetworkExtractAssemblyRequest request)
        {
            _request = request;
            return this;
        }

        protected override Task Write(ZipArchive archive, CancellationToken ct)
        {
            return _writer.WriteAsync(archive, _request, _context, ct);
        }
    }

}
