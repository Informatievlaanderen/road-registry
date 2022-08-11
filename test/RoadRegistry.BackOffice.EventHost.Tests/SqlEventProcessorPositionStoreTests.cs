namespace RoadRegistry.BackOffice.EventHost.Tests;

using AutoFixture;
using Hosts;
using Microsoft.Data.SqlClient;
using RoadRegistry.Framework.Containers;

[Collection(nameof(SqlServerCollection))]
public class SqlEventProcessorPositionStoreTests : IAsyncLifetime
{
    private readonly IFixture _fixture;
    private readonly SqlServer _server;
    private SqlConnectionStringBuilder _builder;

    public SqlEventProcessorPositionStoreTests(SqlServer server)
    {
        _server = server ?? throw new ArgumentNullException(nameof(server));
        _fixture = new Fixture();
    }

    public async Task InitializeAsync()
    {
        _builder = await _server.CreateDatabaseAsync();

        await new SqlEventProcessorPositionStoreSchema(_builder).CreateSchemaIfNotExists("schema");
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    [Fact]
    public async Task ReadVersionInitiallyReturnsExpectedResult()
    {
        var sut = new SqlEventProcessorPositionStore(_builder, "schema");
        var name = _fixture.Create<string>();

        var result = await sut.ReadPosition(name, default);
        Assert.Equal(default, result);
    }

    [Fact]
    public async Task WriteVersionInitiallyHasExpectedResult()
    {
        var sut = new SqlEventProcessorPositionStore(_builder, "schema");
        var name = _fixture.Create<string>();
        var version = _fixture.Create<long>();

        await sut.WritePosition(name, version, default);

        var result = await sut.ReadPosition(name, default);
        Assert.Equal(version, result);
    }

    [Fact]
    public async Task WriteVersionSubsequentlyHasExpectedResult()
    {
        var sut = new SqlEventProcessorPositionStore(_builder, "schema");
        var name = _fixture.Create<string>();
        var previousVersion = _fixture.Create<long>();

        await sut.WritePosition(name, previousVersion, default);

        var nextVersion = _fixture.Create<long>();
        await sut.WritePosition(name, nextVersion, default);

        var result = await sut.ReadPosition(name, default);
        Assert.Equal(nextVersion, result);
    }
}
