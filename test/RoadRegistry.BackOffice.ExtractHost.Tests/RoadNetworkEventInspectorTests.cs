namespace RoadRegistry.BackOffice.ExtractHost.Tests;

using Microsoft.Data.SqlClient;

public class RoadNetworkEventInspectorTests
{
    [Fact(Skip = "Aws debugging")]
    public async Task InspectEvent()
    {
        const string messageId = "8b48b442-5c18-5b8e-8989-a4e4bd23b222";
        const string connectionString = "Server=localhost,9001;Database=road-registry-events;User=basisregisters;Password=;TrustServerCertificate=True;";
        const string tempFilePath = @"eventJsonData.json";

        await using (var connection = new SqlConnection(connectionString))
        await using (var command = connection.CreateCommand())
        {
            await connection.OpenAsync();

            command.CommandText = $"SELECT [JsonData] FROM [road-registry-events].[RoadRegistry].[Messages] WHERE [Id] = '{messageId}'";

            var reader = (string)await command.ExecuteScalarAsync();

            await File.WriteAllTextAsync(tempFilePath, reader);
        }
    }
}