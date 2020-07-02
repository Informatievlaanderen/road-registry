namespace RoadRegistry.Framework.Containers
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Data.SqlClient;

    public class SqlServerEmbeddedContainer : DockerContainer, ISqlServerDatabase
    {
        private const int HostPort = 21533;
        private const string Password = "E@syP@ssw0rd";

        private int _db;

        public SqlServerEmbeddedContainer()
        {
            Configuration = new SqlServerContainerConfiguration(CreateMasterConnectionStringBuilder());
        }

        private SqlConnectionStringBuilder CreateMasterConnectionStringBuilder() =>
            CreateConnectionStringBuilder("master");

        private static SqlConnectionStringBuilder CreateConnectionStringBuilder(string database) =>
            new SqlConnectionStringBuilder
            {
                DataSource = "tcp:localhost," + HostPort,
                InitialCatalog = database,
                UserID = "sa",
                Password = Password,
                Encrypt = false,
                Enlist = false,
                IntegratedSecurity = false
            };

        public async Task<SqlConnectionStringBuilder> CreateDatabaseAsync()
        {
            var database = $"DB{Interlocked.Increment(ref _db)}";
            var text = $@"
CREATE DATABASE [{database}]
ALTER DATABASE [{database}] SET ALLOW_SNAPSHOT_ISOLATION ON
ALTER DATABASE [{database}] SET READ_COMMITTED_SNAPSHOT ON";
            using (var connection = new SqlConnection(CreateMasterConnectionStringBuilder().ConnectionString))
            {
                await connection.OpenAsync();
                using (var command = new SqlCommand(text, connection))
                {
                    await command.ExecuteNonQueryAsync();
                }
                connection.Close();
            }
            return CreateConnectionStringBuilder(database);
        }

        private class SqlServerContainerConfiguration : DockerContainerConfiguration
        {
            public SqlServerContainerConfiguration(SqlConnectionStringBuilder builder)
            {
                Image = new ImageSettings
                {
                    Registry = "mcr.microsoft.com",
                    Name = "mssql/server",
                    Tag = "2019-latest"
                };

                Container = new ContainerSettings
                {
                    Name = "roadregistry-api-db",
                    PortBindings = new[]
                    {
                        new PortBinding
                        {
                            HostPort = HostPort,
                            GuestPort = 1433
                        }
                    },
                    EnvironmentVariables = new[]
                    {
                        "ACCEPT_EULA=Y",
                        $"SA_PASSWORD={Password}"
                    }
                };

                WaitUntilAvailable = async attempt =>
                {
                    if (attempt <= 30)
                    {
                        try
                        {
                            using (var connection = new SqlConnection(builder.ConnectionString))
                            {
                                await connection.OpenAsync();
                                connection.Close();
                            }

                            return TimeSpan.Zero;
                        }
                        catch
                        {
                        }

                        return TimeSpan.FromSeconds(1);
                    }

                    throw new TimeoutException(
                        $"The container {Container.Name} did not become available in a timely fashion.");
                };
            }
        }
    }
}
