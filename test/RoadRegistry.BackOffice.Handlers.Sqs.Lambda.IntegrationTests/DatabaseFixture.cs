namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests;

using Microsoft.Extensions.Configuration;
using Npgsql;

public class DatabaseFixture : IAsyncLifetime
{
    private string _rootConnectionString;

    public Task InitializeAsync()
    {
        _rootConnectionString = new ConfigurationBuilder()
            .AddIntegrationTestAppSettings()
            .Build()
            .GetConnectionString("Marten")!;
        return Task.CompletedTask;
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    public async Task<string> CreateDatabase()
    {
        var databaseName = $"road-{DateTime.Now:yyyyMMdd-HHmmss-fff}";

        await CreateDatabase(databaseName);

        var connectionString = string.Join(';', _rootConnectionString
            .Split(';')
            .Where(x => !x.StartsWith("Database=", StringComparison.InvariantCultureIgnoreCase))
            .Concat([$"Database={databaseName}"]));
        await InstallPostgis(connectionString);

        return connectionString;
    }

    private async Task CreateDatabase(string databaseName)
    {
        var createDbQuery = $"DROP DATABASE IF EXISTS \"{databaseName}\"; CREATE DATABASE \"{databaseName}\";";

        await using var connection = new NpgsqlConnection(_rootConnectionString);
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

    private async Task InstallPostgis(string connectionString)
    {
        await using var connection = new NpgsqlConnection(connectionString);
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
