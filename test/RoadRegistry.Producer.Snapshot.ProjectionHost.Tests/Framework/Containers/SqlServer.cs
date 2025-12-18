namespace RoadRegistry.Producer.Snapshot.ProjectionHost.Tests.Framework.Containers;

using BackOffice;
using BackOffice.Abstractions;
using Infrastructure;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.IO;
using RoadNode;
using RoadRegistry.Tests.Framework.Containers;

public class SqlServer : ISqlServerDatabase
{
    private readonly ISqlServerDatabase _inner;

    public SqlServer()
    {
        _inner = SqlServerDatabaseFactory.Create(RoadRegistryAssembly.ProducerSnapshotProjectionHost);

        MemoryStreamManager = new RecyclableMemoryStreamManager();
        StreetNameCache = new FakeStreetNameCache();
    }

    public RecyclableMemoryStreamManager MemoryStreamManager { get; }
    public IStreetNameCache StreetNameCache { get; }

    public Task<SqlConnectionStringBuilder> CreateDatabaseAsync()
    {
        return _inner.CreateDatabaseAsync();
    }

    public Task DisposeAsync()
    {
        return _inner.DisposeAsync();
    }

    public Task InitializeAsync()
    {
        return _inner.InitializeAsync();
    }

    public async Task<RoadNodeProducerSnapshotContext> CreateEmptyProducerSnapshotContextAsync(SqlConnectionStringBuilder builder)
    {
        var context = await CreateRoadNodeProducerSnapshotContextAsync(builder);

        context.RoadNodes.RemoveRange(context.RoadNodes);

        context.ProjectionStates.RemoveRange(context.ProjectionStates);
        await context.SaveChangesAsync();

        return context;
    }

    public async Task<RoadNodeProducerSnapshotContext> CreateRoadNodeProducerSnapshotContextAsync(SqlConnectionStringBuilder builder)
    {
        var options = new DbContextOptionsBuilder<RoadNodeProducerSnapshotContext>()
            .UseSqlServer(builder.ConnectionString,
                dbContextOptionsBuilder => dbContextOptionsBuilder.UseNetTopologySuite())
            .EnableSensitiveDataLogging()
            .Options;

        var context = new RoadNodeProducerSnapshotContext(options);
        await context.Database.MigrateAsync();
        return context;
    }
}
