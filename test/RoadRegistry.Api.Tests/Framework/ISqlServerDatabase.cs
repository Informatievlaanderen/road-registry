namespace RoadRegistry.Framework
{
    using System.Data.SqlClient;
    using System.Threading.Tasks;
    using Xunit;

    public interface ISqlServerDatabase : IAsyncLifetime
    {
        Task<SqlConnectionStringBuilder> CreateDatabaseAsync();
    }
}
