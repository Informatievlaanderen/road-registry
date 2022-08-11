namespace RoadRegistry.BackOffice.ZipArchiveWriters.Tests.ForProduct;

using System.IO.Compression;
using System.Text;
using Be.Vlaanderen.Basisregisters.Shaperon;
using Product.Schema;
using Product.Schema.GradeSeparatedJunctions;
using RoadRegistry.Framework.Containers;
using Xunit;
using ZipArchiveWriters.ForProduct;
using SqlServer = Framework.Containers.SqlServer;
using SqlServerCollection = Framework.Containers.SqlServerCollection;

[Collection(nameof(SqlServerCollection))]
public class GradeSeparatedJunctionArchiveWriterTests
{
    private readonly SqlServer _fixture;

    public GradeSeparatedJunctionArchiveWriterTests(SqlServer fixture)
    {
        _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
    }

    [Fact]
    public Task ArchiveCanNotBeNull()
    {
        var sut = new GradeSeparatedJunctionArchiveWriter("{0}", _fixture.MemoryStreamManager, Encoding.UTF8);
        return Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.WriteAsync(null, new ProductContext(), default));
    }

    [Fact]
    public Task ContextCanNotBeNull()
    {
        var sut = new GradeSeparatedJunctionArchiveWriter("{0}", _fixture.MemoryStreamManager, Encoding.UTF8);
        return Assert.ThrowsAsync<ArgumentNullException>(
            () => sut.WriteAsync(new ZipArchive(Stream.Null, ZipArchiveMode.Create, true), null, default));
    }

    [Fact]
    public async Task WithEmptyDatabaseWritesArchiveWithExpectedEntries()
    {
        var sut = new GradeSeparatedJunctionArchiveWriter("{0}", _fixture.MemoryStreamManager, Encoding.UTF8);

        var db = await _fixture.CreateDatabaseAsync();
        var context = await _fixture.CreateProductContextAsync(db);

        await new ZipArchiveScenario<ProductContext>(_fixture.MemoryStreamManager, sut)
            .WithContext(context)
            .Assert(readArchive =>
            {
                Assert.Single(readArchive.Entries);
                foreach (var entry in readArchive.Entries)
                    switch (entry.Name)
                    {
                        case "RltOgkruising.dbf":
                            using (var entryStream = entry.Open())
                            using (var reader = new BinaryReader(entryStream, Encoding.UTF8))
                            {
                                Assert.Equal(
                                    new DbaseFileHeader(
                                        DateTime.Now,
                                        DbaseCodePage.Western_European_ANSI,
                                        new DbaseRecordCount(0),
                                        GradeSeparatedJunctionDbaseRecord.Schema),
                                    DbaseFileHeader.Read(reader));
                            }

                            break;

                        default:
                            throw new Exception($"File '{entry.Name}' was not expected in this archive.");
                    }
            });
    }
}
