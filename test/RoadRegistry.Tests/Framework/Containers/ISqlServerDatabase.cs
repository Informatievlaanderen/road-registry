namespace RoadRegistry.Tests.Framework.Containers;

using Microsoft.Data.SqlClient;

public interface ISqlServerDatabase : IAsyncLifetime
{
    Task<SqlConnectionStringBuilder> CreateDatabaseAsync();
}