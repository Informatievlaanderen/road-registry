namespace RoadRegistry.Tests.Framework.Containers;

using Microsoft.Data.SqlClient;

public class SqlServerComposedContainer : ISqlServerDatabase
{
    private const string Password = "E@syP@ssw0rd";

    private readonly SqlConnectionStringBuilder _builder;
    private readonly string _serviceName;
    private int _db;

    public SqlServerComposedContainer(RoadRegistryAssembly serviceName)
    {
        _serviceName = ((int)serviceName).ToString();

        _builder =
            new SqlConnectionStringBuilder
            {
                DataSource = "tcp:localhost,1433",
                InitialCatalog = "master",
                UserID = "sa",
                Password = Password,
                Encrypt = false,
                Enlist = false,
                IntegratedSecurity = false
            };
        ;
    }

    public async Task<SqlConnectionStringBuilder> CreateDatabaseAsync()
    {
        var database = $"DB-{_serviceName}-{Interlocked.Increment(ref _db)}";
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

        return CreateConnectionStringBuilder(database);
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task InitializeAsync()
    {
        var builder = CreateMasterConnectionStringBuilder();

        async Task<TimeSpan> WaitUntilAvailable(int current)
        {
            if (current <= 30)
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
                "The sql server container did not become available in a timely fashion.");
        }

        var attempt = 0;
        var result = await WaitUntilAvailable(attempt++);
        while (result > TimeSpan.Zero)
        {
            await Task.Delay(result);
            result = await WaitUntilAvailable(attempt++);
        }
    }

    private SqlConnectionStringBuilder CreateConnectionStringBuilder(string database)
    {
        return new SqlConnectionStringBuilder(_builder.ConnectionString)
        {
            InitialCatalog = database
        };
    }

    private SqlConnectionStringBuilder CreateMasterConnectionStringBuilder()
    {
        return new SqlConnectionStringBuilder(_builder.ConnectionString)
        {
            InitialCatalog = "master"
        };
    }
}
