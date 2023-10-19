namespace RoadRegistry.BackOffice.ExtractHost.Tests.ZipArchiveWriters;

using System.IO.Compression;
using System.Text;
using Abstractions.Extracts;
using AutoFixture;
using BackOffice.ZipArchiveWriters.ExtractHost;
using Be.Vlaanderen.Basisregisters.BlobStore;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using Extracts;
using Extracts.Dbase;
using FluentAssertions;
using Messages;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO.Streams;
using RoadRegistry.BackOffice.Handlers.Extracts;
using RoadRegistry.BackOffice.ShapeFile;
using RoadRegistry.Tests.BackOffice;
using RoadRegistry.Tests.Framework.Projections;
using Polygon = NetTopologySuite.Geometries.Polygon;

public class TransactionZoneToZipArchiveWriterTests
{
    private readonly Fixture _fixture;
    private readonly IZipArchiveWriter<EditorContext> _sut;

    public TransactionZoneToZipArchiveWriterTests()
    {
        _sut = new TransactionZoneToZipArchiveWriterNetTopologySuite(Encoding.UTF8);
        _fixture = new Fixture();

        CustomizeRoadNetworkExtractAssemblyRequestFixture(_fixture);
    }

    private static EditorContext CreateContextFor(string database)
    {
        var options = new DbContextOptionsBuilder<EditorContext>()
            .UseInMemoryDatabase(database)
            .EnableSensitiveDataLogging()
            .Options;

        return new MemoryEditorContext(options);
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

    private static Polygon ReadGeometryFromArchive(ZipArchive archive, string fileName)
    {
        var archiveFileEntry = archive.GetEntry(fileName);
        Assert.NotNull(archiveFileEntry);
        
        using (var entryStream = archiveFileEntry.Open())
        {
            var ms = new MemoryStream();
            entryStream.CopyTo(ms);

            ms.Position = 0;
            
            var (_, geometry) = new ExtractGeometryShapeFileReader().Read(ms, WellKnownGeometryFactories.Default);
            return (Polygon)geometry;
        }
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

    [Fact]
    public Task GeometryOrdinatesShouldBeOnlyXAndY()
    {
        var editorContext = CreateContextFor(nameof(GeometryOrdinatesShouldBeOnlyXAndY));
        var request = _fixture.Create<RoadNetworkExtractAssemblyRequest>();
        var contour = new Polygon(new LinearRing(new Coordinate[]
        {
            new CoordinateZM(0, 0),
            new CoordinateZM(1, 0),
            new CoordinateZM(1, 1),
            new CoordinateZM(0, 1),
            new CoordinateZM(0, 0)
        }));
        request = new RoadNetworkExtractAssemblyRequest(request.ExternalRequestId, request.DownloadId, request.ExtractDescription, contour);

        Assert.IsType<CoordinateZM>(contour.Coordinate);

        return new ZipArchiveScenarioWithWriter<EditorContext>(new RecyclableMemoryStreamManager(), _sut)
            .WithContext(editorContext)
            .WithRequest(request)
            .Assert(readArchive =>
            {
                var polygon = ReadGeometryFromArchive(readArchive, "Transactiezones.shp");

                var coordinate = Assert.IsType<CoordinateZ>(polygon.Coordinate);
                Assert.Equal(double.NaN, coordinate.M);
                Assert.Equal(double.NaN, coordinate.Z);
            });
    }
}
