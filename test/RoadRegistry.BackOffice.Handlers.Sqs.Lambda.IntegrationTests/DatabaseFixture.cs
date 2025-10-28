namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests;

using Microsoft.Extensions.Configuration;
using Npgsql;

public class DatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; }

    public async Task InitializeAsync()
    {
        var databaseName = $"road-{DateTime.Now:yyyyMMdd-HHmmss-fff}";
        //TODO-pr temp override
        //databaseName = $"road-integrationtests";

        var connectionString = new ConfigurationBuilder()
            .AddIntegrationTestAppSettings()
            .Build()
            .GetConnectionString("Marten")!;

        await CreateDatabase(connectionString, databaseName);

        ConnectionString = string.Join(';', connectionString
            .Split(';')
            .Where(x => !x.StartsWith("Database=", StringComparison.InvariantCultureIgnoreCase))
            .Concat([$"Database={databaseName}"]));

        await InstallPostgis();
    }

    public Task DisposeAsync()
    {
        //TODO-pr drop db?
        return Task.CompletedTask;
    }

    private async Task CreateDatabase(string connectionString, string database)
    {
        var createDbQuery = $"DROP DATABASE IF EXISTS \"{database}\"; CREATE DATABASE \"{database}\";";

        await using var connection = new NpgsqlConnection(connectionString);
        await using var command = new NpgsqlCommand(createDbQuery, connection);

        var attempt = 1;
        while (true)
        {
            try
            {
                await connection.OpenAsync();
                break;
            }
            catch
            {
                if (attempt == 5)
                {
                    throw;
                }

                attempt++;
                await Task.Delay(TimeSpan.FromMilliseconds(200));
            }
        }

        await command.ExecuteNonQueryAsync();
    }

    private async Task InstallPostgis()
    {
        await using var connection = new NpgsqlConnection(ConnectionString);
        await using var command = new NpgsqlCommand("CREATE EXTENSION postgis", connection);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }
}

public static class ConfigurationBuilderExtensions
{
    public static IConfigurationBuilder AddIntegrationTestAppSettings(this IConfigurationBuilder builder)
    {
        return builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.integrationtests.json", optional: false, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false);
    }
}
