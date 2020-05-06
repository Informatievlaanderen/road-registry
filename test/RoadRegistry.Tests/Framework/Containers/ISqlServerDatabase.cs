namespace RoadRegistry.Framework.Containers
{
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;
    using Xunit;

    public interface ISqlServerDatabase : IAsyncLifetime
    {
        Task<SqlConnectionStringBuilder> CreateDatabaseAsync();
    }
}
