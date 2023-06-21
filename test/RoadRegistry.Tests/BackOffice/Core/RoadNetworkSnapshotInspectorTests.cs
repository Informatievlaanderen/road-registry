namespace RoadRegistry.Tests.BackOffice.Core;

using System.Data;
using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using Editor.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RoadRegistry.BackOffice.Messages;

public class RoadNetworkSnapshotInspectorTests
{
    public RoadNetworkSnapshotInspectorTests(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    protected IConfiguration Configuration { get; }

    private string GetEventsConnectionString(DbEnvironment environment)
    {
        return Configuration.GetConnectionString($"Events-{environment}") ?? Configuration.GetConnectionString("Events");
    }
    private string GetEditorProjectionsConnectionString(DbEnvironment environment)
    {
        return Configuration.GetConnectionString($"EditorProjections-{environment}") ?? Configuration.GetConnectionString("EditorProjections");
    }

    [Fact(Skip = "Reads data from EditorContext. Useful for debugging purposes")]
    //[Fact]
    public async Task ReadEditorContext()
    {
        var connectionString = GetEditorProjectionsConnectionString(DbEnvironment.DEV);
        
        using (var dbContext = new EditorContext(new DbContextOptionsBuilder<EditorContext>()
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                   .UseSqlServer(connectionString, options =>
                       options.UseNetTopologySuite()
                   ).Options))
        {
            //var attributes = await dbContext.RoadSegmentNumberedRoadAttributes
            //    .Where(x => x.RoadSegmentId == 268119)
            //    .ToArrayAsync();

            //var records = attributes
            //    .Select(x =>
            //    {
            //        var record = new RoadSegmentNumberedRoadAttributeDbaseRecord();
            //        record.FromBytes(x.DbaseRecord, new RecyclableMemoryStreamManager(), Encoding.UTF8);
            //        return record;
            //    })
            //    .ToArray();

            //var numberedRoads = records.Select(x => x.IDENT8.Value).ToArray();
        }
    }

    [Fact(Skip = "Loads a message to your local computer. Useful for debugging purposes")]
    //[Fact]
    public async Task InspectMessage()
    {
        const int position = 1828076;
        var connectionString = GetEventsConnectionString(DbEnvironment.DEV);
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

                await File.WriteAllTextAsync(messageFilePath, jsonData);
            }
        }
    }

    [Fact(Skip = "Loads a snapshot to your local computer. Useful for debugging purposes")]
    //[Fact]
    public async Task InspectSnapshot()
    {
        const string tempFilePath = @"1828050.bin";

        var snapshotBytes = await File.ReadAllBytesAsync(tempFilePath);
        var snapshot = S3CacheSerializer.Serializer.DeserializeObject<RoadNetworkSnapshot>(snapshotBytes, true).Value;

        
    }

    [Fact(Skip = "Updates the jsondata of a message. Useful for debugging purposes")]
    //[Fact]
    public async Task UpdateMessage()
    {
        const int position = 0;
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

    private enum DbEnvironment
    {
        DEV,
        TST,
        STG,
        PRD
    }
}
