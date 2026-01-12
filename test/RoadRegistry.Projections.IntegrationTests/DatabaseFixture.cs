namespace RoadRegistry.Projections.IntegrationTests;

using Microsoft.Extensions.Configuration;
using Npgsql;
using Polly;

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
            .AddInMemoryCollection([
                new KeyValuePair<string, string?>("ConnectionStrings:Marten", "Host=localhost;port=15433;Username=postgres;Password=postgres")
            ])
            .AddJsonFile("appsettings.integrationtests.json", optional: true, reloadOnChange: false)
            .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false);
    }
}
