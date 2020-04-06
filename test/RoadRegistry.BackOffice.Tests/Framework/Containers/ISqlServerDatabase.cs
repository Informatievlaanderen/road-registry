namespace RoadRegistry.BackOffice.Framework.Containers
{
    using Microsoft.Data.SqlClient;
    using System.Threading.Tasks;
    using Xunit;

    public interface ISqlServerDatabase : IAsyncLifetime
    {
        Task<SqlConnectionStringBuilder> CreateDatabaseAsync();
    }
}
