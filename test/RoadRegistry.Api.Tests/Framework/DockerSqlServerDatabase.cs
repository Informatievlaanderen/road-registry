namespace RoadRegistry.Api.Tests.Framework
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Threading;
    using System.Threading.Tasks;

    public class DockerSqlServerDatabase
    {
        private readonly string _databaseName;
        private readonly DockerContainer _sqlServerContainer;
        private readonly string _password;
        private const string Image = "microsoft/mssql-server-linux";
        private const string Tag = "2017-latest";
        private const int Port = 11433;

        public DockerSqlServerDatabase(string databaseName)
        {
            _databaseName = databaseName;
            _password = "E@syP@ssw0rd";

            var ports = new Dictionary<int, int>
            {
                {1433, Port}
            };

            if (Environment.GetEnvironmentVariable("CI") == null)
            {
                _sqlServerContainer = new DockerContainer(
                    Image,
                    Tag,
                    HealthCheck,
                    ports)
                {
                    ContainerName = "roadregistry-api-tests",
                    Env = new[] {"ACCEPT_EULA=Y", $"SA_PASSWORD={_password}"}
                };
            }
        }

        public SqlConnection CreateMasterConnection()
            => new SqlConnection(CreateMasterConnectionStringBuilder().ConnectionString);

        public SqlConnectionStringBuilder CreateMasterConnectionStringBuilder()
            => Environment.GetEnvironmentVariable("CI") == null
                ? new SqlConnectionStringBuilder(
                    $"server=localhost,{Port};User Id=sa;Password={_password};Initial Catalog=master")
                : new SqlConnectionStringBuilder(
                    $"server=127.0.0.1,1433;User Id=sa;Password={_password};Initial Catalog=master");

        public SqlConnectionStringBuilder CreateConnectionStringBuilder()
            => Environment.GetEnvironmentVariable("CI") == null
                ? new SqlConnectionStringBuilder(
                    $"server=localhost,{Port};User Id=sa;Password={_password};Initial Catalog={_databaseName}")
                : new SqlConnectionStringBuilder(
                    $"server=127.0.0.1,1433;User Id=sa;Password={_password};Initial Catalog={_databaseName}");

        public async Task CreateDatabase(CancellationToken cancellationToken = default)
        {
            if (Environment.GetEnvironmentVariable("CI") == null)
            {
                await _sqlServerContainer.TryStart(cancellationToken);
            }

            using (var connection = CreateMasterConnection())
            {
                await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
                using (var command = new SqlCommand($"CREATE DATABASE [{_databaseName}]", connection))
                {
                    await command.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }

        private async Task<bool> HealthCheck(CancellationToken cancellationToken)
        {
            try
            {
                using (var connection = CreateMasterConnection())
                {
                    await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

                    return true;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }
    }
}
