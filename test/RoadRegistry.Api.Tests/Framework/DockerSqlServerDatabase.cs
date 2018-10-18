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
        private const string Tag = "2017-CU9";
        private const int Port = 11433;

        public DockerSqlServerDatabase(string databaseName)
        {
            _databaseName = databaseName;
            _password = "E@syP@ssw0rd";

            var ports = new Dictionary<int, int>
            {
                {1433, Port}
            };

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

        public SqlConnection CreateMasterConnection()
            => new SqlConnection(CreateMasterConnectionStringBuilder().ConnectionString);

        public SqlConnectionStringBuilder CreateMasterConnectionStringBuilder()
            => new SqlConnectionStringBuilder(
                $"server=localhost,{Port};User Id=sa;Password={_password};Initial Catalog=master");

        public SqlConnection CreateConnection()
            => new SqlConnection(CreateConnectionStringBuilder().ConnectionString);

        public SqlConnectionStringBuilder CreateConnectionStringBuilder()
            => new SqlConnectionStringBuilder(
                $"server=localhost,{Port};User Id=sa;Password={_password};Initial Catalog={_databaseName}");

        public async Task CreateDatabase(CancellationToken cancellationToken = default)
        {
            await _sqlServerContainer.TryStart(cancellationToken);

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
