namespace RoadRegistry.BackOffice.Core
{
    using System.Data;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using MessagePack;
    using Messages;
    using Microsoft.Data.SqlClient;
    using Xunit;

    public class RoadNetworkSnapshotInspectorTests
    {
        [Fact(Skip = "Loads a snapshot to your local computer. Useful for debugging purposes")]
        public async Task InspectSnapshot()
        {
            const string snapshotName = "";
            const string connectionString = "";
            const string tempFilePath = @"snapshot.bin";

            using (var connection = new SqlConnection(connectionString))
            using (var command = connection.CreateCommand())
            {
                await connection.OpenAsync();

                command.CommandText = $"SELECT [Content] FROM [road-registry-events].[RoadRegistrySnapshot].[Blob] WITH (NOLOCK) WHERE [name] = '{snapshotName}'";

                var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
                await reader.ReadAsync();

                using (var tempFile = File.Create(tempFilePath))
                using (var snapshotStream = reader.GetStream(0))
                {
                    await snapshotStream.CopyToAsync(tempFile);
                }
            }

            using (var snapshotStream = File.OpenRead(tempFilePath))
            {
                var snapshot = await MessagePackSerializer.DeserializeAsync<Messages.RoadNetworkSnapshot>(snapshotStream);
            }
        }
    }
}
