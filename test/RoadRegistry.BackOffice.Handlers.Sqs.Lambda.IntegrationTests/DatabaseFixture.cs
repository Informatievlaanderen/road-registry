namespace RoadRegistry.BackOffice.Handlers.Sqs.Lambda.IntegrationTests;

using Microsoft.Extensions.Configuration;
using Npgsql;
using Polly;

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

        await WaitForPgToBeReady(connection);

        await command.ExecuteNonQueryAsync();
    }

    private static async Task WaitForPgToBeReady(NpgsqlConnection connection)
    {
        var policy = Policy
            .Handle<NpgsqlException>()
            .Or<System.Net.Sockets.SocketException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 30,
                _ => TimeSpan.FromSeconds(1));
        await policy.ExecuteAsync(connection.OpenAsync);
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
            .AddInMemoryCollection([
                new KeyValuePair<string, string?>("ConnectionStrings:Marten", "Host=localhost;port=15432;Username=postgres;Password=postgres")
            ])
            .AddJsonFile("appsettings.integrationtests.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false);
    }
}
