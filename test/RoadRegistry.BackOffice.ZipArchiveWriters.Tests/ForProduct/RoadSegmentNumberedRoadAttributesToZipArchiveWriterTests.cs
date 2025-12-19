namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.ForProduct;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Framework.Containers;
using Product.Schema;
using RoadRegistry.Extracts.Schemas.ExtractV1.RoadSegments;
using ZipArchiveWriters.ForProduct;

[Collection(nameof(SqlServerCollection))]
public class RoadSegmentNumberedRoadAttributesToZipArchiveWriterTests
{
    private readonly SqlServer _fixture;

    public RoadSegmentNumberedRoadAttributesToZipArchiveWriterTests(SqlServer fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    [Fact]
    public Task ArchiveCanNotBeNull()
    {
        var sut = new RoadSegmentNumberedRoadAttributesToZipArchiveWriter("{0}", _fixture.MemoryStreamManager, Encoding.UTF8);
        return Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.WriteAsync(null, new ProductContext(), default));
    }

    [Fact]
    public Task ContextCanNotBeNull()
    {
        var sut = new RoadSegmentNumberedRoadAttributesToZipArchiveWriter("{0}", _fixture.MemoryStreamManager, Encoding.UTF8);
        return Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.WriteAsync(new ZipArchive(Stream.Null, ZipArchiveMode.Create, true), null, default));
    }

    [Fact]
    public async Task WithEmptyDatabaseWritesArchiveWithExpectedEntries()
    {
        var sut = new RoadSegmentNumberedRoadAttributesToZipArchiveWriter("{0}", _fixture.MemoryStreamManager, Encoding.UTF8);

        var db = await _fixture.CreateDatabaseAsync();
        var context = await _fixture.CreateProductContextAsync(db);
        await context.SaveChangesAsync();

        await new ZipArchiveScenario<ProductContext>(_fixture.MemoryStreamManager, sut)
            .WithContext(context)
            .Assert(readArchive =>
            {
                Assert.Single(readArchive.Entries);
                foreach (var entry in readArchive.Entries)
                {
                    switch (entry.Name)
                    {
                        case "AttGenumweg.dbf":
                            using (var entryStream = entry.Open())
                            using (var reader = new BinaryReader(entryStream, Encoding.UTF8))
                            {
                                Assert.Equal(
                                    new DbaseFileHeader(
                                        DateTime.Now,
                                        DbaseCodePage.Western_European_ANSI,
                                        new DbaseRecordCount(0),
                                        RoadSegmentNumberedRoadAttributeDbaseRecord.Schema),
                                    DbaseFileHeader.Read(reader));
                            }

                            break;

                        default:
                            throw new Exception($"File '{entry.Name}' was not expected in this archive.");
                    }
                }
            });
    }
}
