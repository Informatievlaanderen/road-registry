namespace RoadRegistry.BackOffice.CommandHost
{
    using System;
    using Microsoft.Data.SqlClient;
    using System.Threading.Tasks;
    using AutoFixture;
    using Framework.Containers;
    using Xunit;

    [Collection(nameof(SqlServerCollection))]
    public class SqlCommandProcessorPositionStoreTests : IAsyncLifetime
    {
        private readonly IFixture _fixture;
        private readonly SqlServer _server;
        private SqlConnectionStringBuilder _builder;

        public SqlCommandProcessorPositionStoreTests(SqlServer server)
        {
            _server = server ?? throw new ArgumentNullException(nameof(server));
            _fixture = new Fixture();
        }

        public async Task InitializeAsync()
        {
            _builder = await _server.CreateDatabaseAsync();

            await new SqlCommandProcessorPositionStoreSchema(_builder).CreateSchemaIfNotExists("schema");
        }

        public Task DisposeAsync()
        {
            return Task.CompletedTask;
        }

        [Fact]
        public async Task ReadVersionInitiallyReturnsExpectedResult()
        {
            var sut = new SqlCommandProcessorPositionStore(_builder, "schema");
            var name = _fixture.Create<string>();

            var result = await sut.ReadVersion(name, default);
            Assert.Equal(default, result);
        }

        [Fact]
        public async Task WriteVersionInitiallyHasExpectedResult()
        {
            var sut = new SqlCommandProcessorPositionStore(_builder, "schema");
            var name = _fixture.Create<string>();
            var version = _fixture.Create<int>();

            await sut.WriteVersion(name, version, default);

            var result = await sut.ReadVersion(name, default);
            Assert.Equal(version, result);
        }

        [Fact]
        public async Task WriteVersionSubsequentlyHasExpectedResult()
        {
            var sut = new SqlCommandProcessorPositionStore(_builder, "schema");
            var name = _fixture.Create<string>();
            var previousVersion = _fixture.Create<int>();

            await sut.WriteVersion(name, previousVersion, default);

            var nextVersion = _fixture.Create<int>();
            await sut.WriteVersion(name, nextVersion, default);

            var result = await sut.ReadVersion(name, default);
            Assert.Equal(nextVersion, result);
        }
    }
}
