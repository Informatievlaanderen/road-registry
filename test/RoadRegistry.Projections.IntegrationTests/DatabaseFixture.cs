namespace RoadRegistry.Projections.IntegrationTests;

using Microsoft.Extensions.Configuration;
using Npgsql;

public class DatabaseFixture : IAsyncLifetime
{
    public string DatabaseName { get; private set; }
    public string ConnectionString { get; private set; }
    private string _rootConnectionString;

    public async Task InitializeAsync()
    {
        DatabaseName = $"road-{DateTime.Now:yyyyMMdd-HHmmss-fff}";

        _rootConnectionString = new ConfigurationBuilder()
            .AddIntegrationTestAppSettings()
            .Build()
            .GetConnectionString("Marten")!;

        await CreateDatabase();

        ConnectionString = string.Join(';', _rootConnectionString
            .Split(';')
            .Where(x => !x.StartsWith("Database=", StringComparison.InvariantCultureIgnoreCase))
            .Concat([$"Database={DatabaseName}"]));

        await InstallPostgis();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    private async Task CreateDatabase()
    {
        var createDbQuery = $"DROP DATABASE IF EXISTS \"{DatabaseName}\"; CREATE DATABASE \"{DatabaseName}\";";

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
