namespace RoadRegistry.Api.Tests
{
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Threading.Tasks;
    using Downloads;
    using Framework;
    using Projections;
    using Xunit;

    [Collection(nameof(SqlServerDatabaseCollection))]
    public class RoadNodeArchiveWriterTests
    {
        private readonly SqlServerDatabaseFixture _fixture;

        public RoadNodeArchiveWriterTests(SqlServerDatabaseFixture fixture)
        {
            _fixture = fixture;
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
