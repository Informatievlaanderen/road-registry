namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.ForEditor;

using System.IO.Compression;
using System.Text;
using Abstractions;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using Framework.Containers;
using RoadRegistry.Extracts;
using RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;
using RoadRegistry.Extracts.ZipArchiveWriters;
using ZipArchiveWriters.ForEditor;

[Collection(nameof(SqlServerCollection))]
public class RoadSegmentsArchiveWriterTests
{
    private readonly SqlServer _fixture;
    private readonly FileEncoding _fileEncoding;

    public RoadSegmentsArchiveWriterTests(SqlServer fixture, FileEncoding fileEncoding)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        _fileEncoding = fileEncoding;
    }

    [Fact]
    public Task ArchiveCanNotBeNull()
    {
        var sut = new RoadSegmentsToZipArchiveWriter(new ZipArchiveWriterOptions(), _fixture.StreetNameCache, _fixture.MemoryStreamManager, _fileEncoding);
        return Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.WriteAsync(null, new EditorContext(), default));
    }

    [Fact]
    public Task ContextCanNotBeNull()
    {
        var sut = new RoadSegmentsToZipArchiveWriter(new ZipArchiveWriterOptions(), _fixture.StreetNameCache, _fixture.MemoryStreamManager, _fileEncoding);
        return Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.WriteAsync(new ZipArchive(Stream.Null, ZipArchiveMode.Create, true), null, default));
    }

    [Fact]
    public async Task WithEmptyRoadNetworkWritesArchiveWithExpectedEntries()
    {
        var sut = new RoadSegmentsToZipArchiveWriter(new ZipArchiveWriterOptions(), _fixture.StreetNameCache, _fixture.MemoryStreamManager, _fileEncoding);

        var db = await _fixture.CreateDatabaseAsync();
        var context = await _fixture.CreateEditorContextAsync(db);
        await context.RoadNetworkInfo.AddAsync(new RoadNetworkInfo
        {
            CompletedImport = true,
            TotalRoadNodeShapeLength = 0
        });
        await context.SaveChangesAsync();

        await new ZipArchiveScenario<EditorContext>(_fixture.MemoryStreamManager, sut)
            .WithContext(context)
            .Assert(readArchive =>
            {
                Assert.Equal(4, readArchive.Entries.Count);
                foreach (var entry in readArchive.Entries)
                {
                    switch (entry.Name)
                    {
                        case "Wegsegment.dbf":
                            using (var entryStream = entry.Open())
                            using (var reader = new BinaryReader(entryStream, _fileEncoding))
                            {
                                Assert.Equal(
                                    new DbaseFileHeader(
                                        DateTime.Now,
                                        DbaseCodePage.Western_European_ANSI,
                                        new DbaseRecordCount(0),
                                        RoadSegmentDbaseRecord.Schema),
                                    DbaseFileHeader.Read(reader));
                            }

                            break;

                        case "Wegsegment.shp":
                            using (var entryStream = entry.Open())
                            using (var reader = new BinaryReader(entryStream, _fileEncoding))
                            {
                                Assert.Equal(
                                    new ShapeFileHeader(
                                        new WordLength(0),
                                        ShapeType.PolyLineM,
                                        BoundingBox3D.Empty),
                                    ShapeFileHeader.Read(reader));
                            }

                            break;

                        case "Wegsegment.shx":
                            using (var entryStream = entry.Open())
                            using (var reader = new BinaryReader(entryStream, _fileEncoding))
                            {
                                Assert.Equal(
                                    new ShapeFileHeader(
                                        ShapeFileHeader.Length,
                                        ShapeType.PolyLineM,
                                        BoundingBox3D.Empty),
                                    ShapeFileHeader.Read(reader));
                            }

                            break;

                        case "Wegsegment.cpg":
                            using (var entryStream = entry.Open())
                            using (var reader = new StreamReader(entryStream, _fileEncoding))
                            {
                                Assert.Equal(
                                    _fileEncoding.Encoding.CodePage.ToString(),
                                    reader.ReadToEnd());
                            }

                            break;

                        default:
                            throw new Exception($"File '{entry.Name}' was not expected in this archive.");
                    }
                }
            });
    }

    [Fact(Skip = "Complete once value objects become available")]
    public Task WriteAsyncHasExpectedResult()
    {
        return Task.CompletedTask;
    }
}
