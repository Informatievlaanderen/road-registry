namespace RoadRegistry.Tests.BackOffice.Core;

using System.Data;
using MessagePack;
using Microsoft.Data.SqlClient;
using RoadRegistry.BackOffice.Messages;
using Xunit;

public class RoadNetworkSnapshotInspectorTests
{
    [Fact(Skip = "Loads a snapshot to your local computer. Useful for debugging purposes")]
    public async Task InspectSnapshot()
    {
        const string snapshotName = "";
        const string connectionString = "";
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
}
