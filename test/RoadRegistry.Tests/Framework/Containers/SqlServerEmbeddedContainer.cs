namespace RoadRegistry.Tests.Framework.Containers;

using Microsoft.Data.SqlClient;

public class SqlServerEmbeddedContainer : DockerContainer, ISqlServerDatabase
{
    private const string Password = "E@syP@ssw0rd";

    private int _hostPort;
    private int _db;

    public SqlServerEmbeddedContainer(int hostPort)
    {
        _hostPort = hostPort;
        Configuration = new SqlServerContainerConfiguration(CreateMasterConnectionStringBuilder(), _hostPort);
    }

    public async Task<SqlConnectionStringBuilder> CreateDatabaseAsync()
    {
        var database = $"DB{Interlocked.Increment(ref _db)}";
        var text = $@"
CREATE DATABASE [{database}]
ALTER DATABASE [{database}] SET ALLOW_SNAPSHOT_ISOLATION ON
ALTER DATABASE [{database}] SET READ_COMMITTED_SNAPSHOT ON";
        await using (var connection = new SqlConnection(CreateMasterConnectionStringBuilder().ConnectionString))
        {
            await connection.OpenAsync();
            await using (var command = new SqlCommand(text, connection))
            {
                await command.ExecuteNonQueryAsync();
            }

            connection.Close();
        }

        return CreateConnectionStringBuilder(database, _hostPort);
    }

    private SqlConnectionStringBuilder CreateMasterConnectionStringBuilder()
    {
        return CreateConnectionStringBuilder("master", _hostPort);
    }

    private static SqlConnectionStringBuilder CreateConnectionStringBuilder(string database, int hostPort)
    {
        return new SqlConnectionStringBuilder
        {
            DataSource = "tcp:localhost," + hostPort,
            InitialCatalog = database,
            UserID = "sa",
            Password = Password,
            Encrypt = false,
            Enlist = false,
            IntegratedSecurity = false
        };
    }

    private class SqlServerContainerConfiguration : DockerContainerConfiguration
    {
        public SqlServerContainerConfiguration(SqlConnectionStringBuilder builder, int hostPort)
        {
            Image = new ImageSettings
            {
                Registry = "mcr.microsoft.com",
                Name = "mssql/server",
                Tag = "2019-latest"
            };

            Container = new ContainerSettings
            {
                Name = "road-registry-test-db",
                PortBindings = new[]
                {
                    new PortBinding
                    {
                        HostPort = hostPort,
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
                        await using (var connection = new SqlConnection(builder.ConnectionString))
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
