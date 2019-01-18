namespace RoadRegistry.BackOffice.Api
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Threading.Tasks;
    using Framework.Containers;
    using RoadRegistry.Api.Downloads;
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
            var sut = new RoadNodeArchiveWriter();
            return Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.WriteAsync(null, new ShapeContext()));
        }

        [Fact]
        public Task ContextCanNotBeNull()
        {
            var sut = new RoadNodeArchiveWriter();
            return Assert.ThrowsAsync<ArgumentNullException>(
                () => sut.WriteAsync(new ZipArchive(Stream.Null, ZipArchiveMode.Create, true), null));
        }

        [Fact(Skip = "Complete once value objects become available")]
        public Task WriteAsyncHasExpectedResult()
        {
            return Task.CompletedTask;
        }
    }
}
