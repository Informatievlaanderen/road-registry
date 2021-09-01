namespace RoadRegistry.BackOffice.Api.ZipArchiveWriters
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using BackOffice.Uploads;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.IO;
    using Xunit;

    public class ProjectionFormatFileZipArchiveWriterTests
    {
        [Fact]
        public void FileNameCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProjectionFormatFileZipArchiveWriter<FakeDbContext>(null, Encoding.Default));
        }

        [Fact]
        public void EncodingCanNotBeNull()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new ProjectionFormatFileZipArchiveWriter<FakeDbContext>("file.prj", null));
        }

        [Fact]
        public void ArchiveCanNotBeNull()
        {
            var sut = new ProjectionFormatFileZipArchiveWriter<FakeDbContext>("file.prj", Encoding.Default);
            Assert.ThrowsAsync<ArgumentNullException>(() => sut.WriteAsync(null, new FakeDbContext(), CancellationToken.None));
        }

        [Fact]
        public void ContextCanNotBeNull()
        {
            var sut = new ProjectionFormatFileZipArchiveWriter<FakeDbContext>("file.prj", Encoding.Default);
            Assert.ThrowsAsync<ArgumentNullException>(() => sut.WriteAsync(new ZipArchive(Stream.Null, ZipArchiveMode.Create, true), null, CancellationToken.None));
        }

        [Fact]
        public Task WritesExpectedEntry()
        {
            var sut = new ProjectionFormatFileZipArchiveWriter<FakeDbContext>("file.prj", Encoding.Default);
            return new ZipArchiveScenario<FakeDbContext>(new RecyclableMemoryStreamManager(), sut)
                .WithContext(new FakeDbContext())
                .Assert(readArchive =>
                {
                    Assert.Single(readArchive.Entries);
                    foreach (var entry in readArchive.Entries)
                    {
                        switch (entry.Name)
                        {
                            case "file.prj":
                                using (var entryStream = entry.Open())
                                using (var entryStreamReader = new StreamReader(entryStream, Encoding.Default))
                                {
                                    var result = entryStreamReader.ReadToEnd();
                                    Assert.Equal(ProjectionFormat.Belge_Lambert_1972, result);
                                }
                                break;

                            default:
                                throw new Exception($"File '{entry.Name}' was not expected in this archive.");
                        }
                    }
                });
        }

        private class FakeDbContext : DbContext{}
    }
}
