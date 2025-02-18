namespace RoadRegistry.BackOffice.Handlers.Sqs.Tests.Framework.Containers;

using Microsoft.Data.SqlClient;
using RoadRegistry.Tests.Framework.Containers;

public class SqlServer : ISqlServerDatabase
{
    private readonly ISqlServerDatabase _inner;

    public SqlServer()
    {
        _inner = SqlServerDatabaseFactory.Create(RoadRegistryAssembly.BackOfficeHandlersSqs);
    }

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
