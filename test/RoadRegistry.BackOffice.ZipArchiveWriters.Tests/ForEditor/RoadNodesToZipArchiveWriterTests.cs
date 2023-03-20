namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.ForEditor;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Editor.Schema;
using Extracts.Dbase.RoadNodes;
using Framework.Containers;
using ZipArchiveWriters.ForEditor;

[Collection(nameof(SqlServerCollection))]
public class RoadNodesToZipArchiveWriterTests
{
    private readonly SqlServer _fixture;

    public RoadNodesToZipArchiveWriterTests(SqlServer fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    [Fact]
    public Task ArchiveCanNotBeNull()
    {
        var sut = new RoadNodesToZipArchiveWriter(_fixture.MemoryStreamManager, Encoding.UTF8);
        return Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.WriteAsync(null, new EditorContext(), default));
    }

    [Fact]
    public Task ContextCanNotBeNull()
    {
        var sut = new RoadNodesToZipArchiveWriter(_fixture.MemoryStreamManager, Encoding.UTF8);
        return Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.WriteAsync(new ZipArchive(Stream.Null, ZipArchiveMode.Create, true), null, default));
    }

    [Fact]
    public async Task WithEmptyRoadNetworkWritesArchiveWithExpectedEntries()
    {
        var sut = new RoadNodesToZipArchiveWriter(_fixture.MemoryStreamManager, Encoding.UTF8);

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
                Assert.Equal(3, readArchive.Entries.Count);
                foreach (var entry in readArchive.Entries)
                    switch (entry.Name)
                    {
                        case "Wegknoop.dbf":
                            using (var entryStream = entry.Open())
                            using (var reader = new BinaryReader(entryStream, Encoding.UTF8))
                            {
                                Assert.Equal(
                                    new DbaseFileHeader(
                                        DateTime.Now,
                                        DbaseCodePage.Western_European_ANSI,
                                        new DbaseRecordCount(0),
                                        RoadNodeDbaseRecord.Schema),
                                    DbaseFileHeader.Read(reader));
                            }

                            break;

                        case "Wegknoop.shp":
                            using (var entryStream = entry.Open())
                            using (var reader = new BinaryReader(entryStream, Encoding.UTF8))
                            {
                                Assert.Equal(
                                    new ShapeFileHeader(
                                        new WordLength(0),
                                        ShapeType.Point,
                                        BoundingBox3D.Empty),
                                    ShapeFileHeader.Read(reader));
                            }

                            break;

                        case "Wegknoop.shx":
                            using (var entryStream = entry.Open())
                            using (var reader = new BinaryReader(entryStream, Encoding.UTF8))
                            {
                                Assert.Equal(
                                    new ShapeFileHeader(
                                        ShapeFileHeader.Length,
                                        ShapeType.Point,
                                        BoundingBox3D.Empty),
                                    ShapeFileHeader.Read(reader));
                            }

                            break;

                        default:
                            throw new Exception($"File '{entry.Name}' was not expected in this archive.");
                    }
            });
    }

    [Fact(Skip = "Complete once value objects become available")]
    public Task WriteAsyncHasExpectedResult()
    {
        return Task.CompletedTask;
    }
}
