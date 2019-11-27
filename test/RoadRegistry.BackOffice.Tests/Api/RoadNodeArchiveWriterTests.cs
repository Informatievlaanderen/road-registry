namespace RoadRegistry.BackOffice.Api
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading.Tasks;
    using Framework.Containers;
    using Microsoft.IO;
    using RoadRegistry.Api.ZipArchiveWriters;
    using Schema;
    using Xunit;

    [Collection(nameof(SqlServerCollection))]
    public class RoadNodeArchiveWriterTests
    {
        private readonly SqlServer _fixture;

        public RoadNodeArchiveWriterTests(SqlServer fixture)
        {
            _fixture = fixture ?? throw new ArgumentNullException(nameof(fixture));
        }

        [Fact]
        public Task ArchiveCanNotBeNull()
        {
            var sut = new RoadNodesToZipArchiveWriter(new RecyclableMemoryStreamManager(),  Encoding.UTF8);
            return Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.WriteAsync(null, new ShapeContext(), default));
        }

        [Fact]
        public Task ContextCanNotBeNull()
        {
            var sut = new RoadNodesToZipArchiveWriter(new RecyclableMemoryStreamManager(), Encoding.UTF8);
            return Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.WriteAsync(new ZipArchive(Stream.Null, ZipArchiveMode.Create, true), null, default));
        }

        [Fact(Skip = "Complete once value objects become available")]
        public Task WriteAsyncHasExpectedResult()
        {
            return Task.CompletedTask;
        }
    }
}
