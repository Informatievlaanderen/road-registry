namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters.ForProduct
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Configuration;
    using Product.Schema;
    using Product.Schema.RoadSegments;
    using RoadRegistry.Framework.Containers;
    using Xunit;

    [Collection(nameof(SqlServerCollection))]
    public class RoadSegmentsArchiveWriterTests
    {
        private readonly SqlServer _fixture;

        public RoadSegmentsArchiveWriterTests(SqlServer fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public Task ArchiveCanNotBeNull()
        {
            var sut = new RoadSegmentsToZipArchiveWriter(new ZipArchiveWriterOptions(), new StreetNameCacheStub(),  _fixture.MemoryStreamManager,  Encoding.UTF8);
            return Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.WriteAsync(null, new ProductContext(), default));
        }

        [Fact]
        public Task ContextCanNotBeNull()
        {
            var sut = new RoadSegmentsToZipArchiveWriter(new ZipArchiveWriterOptions(), new StreetNameCacheStub(),  _fixture.MemoryStreamManager, Encoding.UTF8);
            return Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.WriteAsync(new ZipArchive(Stream.Null, ZipArchiveMode.Create, true), null, default));
        }

        [Fact(Skip = "Complete once value objects become available")]
        public Task WriteAsyncHasExpectedResult()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task WithEmptyRoadNetworkWritesArchiveWithExpectedEntries()
        {
            var sut = new RoadSegmentsToZipArchiveWriter(new ZipArchiveWriterOptions(), new StreetNameCacheStub(),  _fixture.MemoryStreamManager, Encoding.UTF8);

            var db = await _fixture.CreateDatabaseAsync();
            var context = await _fixture.CreateProductContextAsync(db);
            await context.RoadNetworkInfo.AddAsync(new RoadNetworkInfo
            {
                CompletedImport = true,
                TotalRoadNodeShapeLength = 0,
            });
            await context.SaveChangesAsync();

            await new ZipArchiveScenario<ProductContext>(_fixture, sut)
                .WithContext(context)
                .Assert(readArchive =>
                {
                    Assert.Equal(3, readArchive.Entries.Count);
                    foreach (var entry in readArchive.Entries)
                    {
                        switch (entry.Name)
                        {
                            case "Wegsegment.dbf":
                                using (var entryStream = entry.Open())
                                using (var reader = new BinaryReader(entryStream, Encoding.UTF8))
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
                                using (var reader = new BinaryReader(entryStream, Encoding.UTF8))
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
                                using (var reader = new BinaryReader(entryStream, Encoding.UTF8))
                                {
                                    Assert.Equal(
                                        new ShapeFileHeader(
                                            ShapeFileHeader.Length,
                                            ShapeType.PolyLineM,
                                            BoundingBox3D.Empty),
                                        ShapeFileHeader.Read(reader));
                                }

                                break;

                            default:
                                throw new Exception($"File '{entry.Name}' was not expected in this archive.");
                        }
                    }
                });
        }
    }
}
