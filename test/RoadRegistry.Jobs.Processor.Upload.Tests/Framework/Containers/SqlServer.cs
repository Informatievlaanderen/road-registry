namespace RoadRegistry.Jobs.Processor.Upload.Tests.Framework.Containers;

using BackOffice;
using Microsoft.Data.SqlClient;
using Microsoft.IO;
using RoadRegistry.Tests.Framework.Containers;

public class SqlServer : ISqlServerDatabase
{
    private readonly ISqlServerDatabase _inner;

    public SqlServer()
    {
        _inner = SqlServerDatabaseFactory.Create(RoadRegistryAssembly.JobsProcessorUpload);

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
}
