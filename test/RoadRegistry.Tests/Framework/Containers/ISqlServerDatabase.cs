namespace RoadRegistry.Tests.Framework.Containers;

using Microsoft.Data.SqlClient;
using Xunit;

public interface ISqlServerDatabase : IAsyncLifetime
{
    Task<SqlConnectionStringBuilder> CreateDatabaseAsync();
}
