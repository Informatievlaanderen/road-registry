﻿namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters.ForEditor
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Editor.Schema;
    using Editor.Schema.RoadSegments;
    using RoadRegistry.Framework.Containers;
    using Xunit;

    [Collection(nameof(SqlServerCollection))]
    public class RoadSegmentWidthAttributesToZipArchiveWriterTests
    {
        private readonly SqlServer _fixture;

        public RoadSegmentWidthAttributesToZipArchiveWriterTests(SqlServer fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public Task ArchiveCanNotBeNull()
        {
            var sut = new RoadSegmentWidthAttributesToZipArchiveWriter(_fixture.MemoryStreamManager, Encoding.UTF8);
            return Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.WriteAsync(null, new EditorContext(), default));
        }

        [Fact]
        public Task ContextCanNotBeNull()
        {
            var sut = new RoadSegmentWidthAttributesToZipArchiveWriter(_fixture.MemoryStreamManager, Encoding.UTF8);
            return Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.WriteAsync(new ZipArchive(Stream.Null, ZipArchiveMode.Create, true), null, default));
        }

        [Fact]
        public async Task WithEmptyDatabaseWritesArchiveWithExpectedEntries()
        {
            var sut = new RoadSegmentWidthAttributesToZipArchiveWriter(_fixture.MemoryStreamManager, Encoding.UTF8);

            var db = await _fixture.CreateDatabaseAsync();
            var context = await _fixture.CreateEditorContextAsync(db);
            await context.SaveChangesAsync();

            await new ZipArchiveScenario<EditorContext>(_fixture, sut)
                .WithContext(context)
                .Assert(readArchive =>
                {
                    Assert.Single(readArchive.Entries);
                    foreach (var entry in readArchive.Entries)
                    {
                        switch (entry.Name)
                        {
                            case "AttWegbreedte.dbf":
                                using (var entryStream = entry.Open())
                                using (var reader = new BinaryReader(entryStream, Encoding.UTF8))
                                {
                                    Assert.Equal(
                                        new DbaseFileHeader(
                                            DateTime.Now,
                                            DbaseCodePage.Western_European_ANSI,
                                            new DbaseRecordCount(0),
                                            RoadSegmentWidthAttributeDbaseRecord.Schema),
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
