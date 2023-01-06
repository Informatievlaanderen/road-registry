namespace RoadRegistry.Tests.BackOffice.Core;

using System.Data;
using MessagePack;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using RoadRegistry.BackOffice.Messages;
using Xunit;

public class RoadNetworkSnapshotInspectorTests
{
    protected IConfiguration Configuration { get; }

    public RoadNetworkSnapshotInspectorTests(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    [Fact(Skip = "Loads a snapshot to your local computer. Useful for debugging purposes")]
    //[Fact]
    public async Task InspectSnapshot()
    {
        const string snapshotName = "";
        var connectionString = GetEventsConnectionString(DbEnvironment.DEV);
        const string tempFilePath = @"snapshot.bin";

        await using (var connection = new SqlConnection(connectionString))
        await using (var command = connection.CreateCommand())
        {
            await connection.OpenAsync();

            command.CommandText = $"SELECT [Content] FROM [road-registry-events].[RoadRegistrySnapshot].[Blob] WITH (NOLOCK) WHERE [name] = '{snapshotName}'";

            var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
            await reader.ReadAsync();

            await using (var tempFile = File.Create(tempFilePath))
            await using (var snapshotStream = reader.GetStream(0))
            {
                await snapshotStream.CopyToAsync(tempFile);
            }
        }

        await using (var snapshotStream = File.OpenRead(tempFilePath))
        {
            var snapshot = await MessagePackSerializer.DeserializeAsync<RoadNetworkSnapshot>(snapshotStream);
        }
    }

    [Fact(Skip = "Loads a message to your local computer. Useful for debugging purposes")]
    //[Fact]
    public async Task InspectMessage()
    {
        const int position = 0;
        var connectionString = GetEventsConnectionString(DbEnvironment.TST);
        var messageFilePath = $"message-{position}.json";

        await using (var connection = new SqlConnection(connectionString))
        await using (var command = connection.CreateCommand())
        {
            await connection.OpenAsync();

            command.CommandText = $"SELECT [JsonData] FROM [road-registry-events].[RoadRegistry].[Messages] WITH (NOLOCK) WHERE [Position] = {position}";

            var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
            await reader.ReadAsync();

            Assert.True(reader.HasRows);

            if (reader.HasRows)
            {
                var jsonData = reader.GetString(0);
                Assert.NotNull(jsonData);

                await File.WriteAllTextAsync(messageFilePath, reader.GetString(0));
            }
        }
    }

    [Fact(Skip = "Updates the jsondata of a message. Useful for debugging purposes")]
    //[Fact]
    public async Task UpdateMessage()
    {
        const int position = 0; // 1825727 1825873
        var connectionString = GetEventsConnectionString(DbEnvironment.TST);
        var messageFilePath = $"message-{position}.json";

        var jsonData = await File.ReadAllTextAsync(messageFilePath);

        await using (var connection = new SqlConnection(connectionString))
        await using (var command = connection.CreateCommand())
        {
            await connection.OpenAsync();

            command.CommandText = $"UPDATE [road-registry-events].[RoadRegistry].[Messages] SET [JsonData] = @JsonData WHERE [Position] = {position}";
            var parameter = command.Parameters.Add("JsonData", SqlDbType.Text);
            parameter.Value = jsonData;

            var result = await command.ExecuteNonQueryAsync();
            Assert.Equal(1, result);
        }
    }

    private string GetEventsConnectionString(DbEnvironment environment)
    {
        return Configuration.GetConnectionString($"Events-{environment}") ?? Configuration.GetConnectionString("Events");
    }

    private enum DbEnvironment
    {
        DEV,
        TST,
        STG,
        PRD
    }
}
