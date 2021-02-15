namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters.ForProduct
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Core;
    using Product.Schema;
    using Product.Schema.Organizations;
    using RoadRegistry.Framework.Containers;
    using Xunit;

    [Collection(nameof(SqlServerCollection))]
    public class OrganizationsToZipArchiveWriterTests
    {
        private readonly SqlServer _fixture;

        public OrganizationsToZipArchiveWriterTests(SqlServer fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public Task ArchiveCanNotBeNull()
        {
            var sut = new OrganizationsToZipArchiveWriter(_fixture.MemoryStreamManager, Encoding.UTF8);
            return Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.WriteAsync(null, new ProductContext(), default));
        }

        [Fact]
        public Task ContextCanNotBeNull()
        {
            var sut = new OrganizationsToZipArchiveWriter(_fixture.MemoryStreamManager, Encoding.UTF8);
            return Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.WriteAsync(new ZipArchive(Stream.Null, ZipArchiveMode.Create, true), null, default));
        }

        [Fact]
        public async Task WithEmptyDatabaseWritesArchiveWithExpectedEntries()
        {
            var sut = new OrganizationsToZipArchiveWriter(_fixture.MemoryStreamManager, Encoding.UTF8);

            var db = await _fixture.CreateDatabaseAsync();
            var context = await _fixture.CreateProductContextAsync(db);

            await new ZipArchiveScenario<ProductContext>(_fixture.MemoryStreamManager, sut)
                .WithContext(context)
                .Assert(readArchive =>
                {
                    Assert.Single(readArchive.Entries);
                    foreach (var entry in readArchive.Entries)
                    {
                        switch (entry.Name)
                        {
                            case "LstOrg.dbf":
                                using (var entryStream = entry.Open())
                                using (var reader = new BinaryReader(entryStream, Encoding.UTF8))
                                {
                                    Assert.Equal(
                                        new DbaseFileHeader(
                                            DateTime.Now,
                                            DbaseCodePage.Western_European_ANSI,
                                            new DbaseRecordCount(0 + Organization.PredefinedTranslations.All.Length),
                                            OrganizationDbaseRecord.Schema),
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
}
