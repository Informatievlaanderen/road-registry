namespace RoadRegistry.Tests.BackOffice.Core;

using System.Data;
using System.Text;
using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Editor.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Polly;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using Serilog.Enrichers;
using SqlStreamStore;

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

    private IStreamStore GetStreamStore(DbEnvironment dbEnvironment)
    {
        var connectionString = GetEventsConnectionString(dbEnvironment);
        return new MsSqlStreamStoreV3(
            new MsSqlStreamStoreV3Settings(
                connectionString)
            {
                Schema = WellknownSchemas.EventSchema
            });
    }
    private EditorContext GetEditorContext(DbEnvironment dbEnvironment)
    {
        return new EditorContext(new DbContextOptionsBuilder<EditorContext>()
                   .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                   .UseSqlServer(GetEditorProjectionsConnectionString(dbEnvironment), options =>
                       options.UseNetTopologySuite()
                   ).Options);
    }

    [Fact(Skip = "Reads data from EditorContext. Useful for debugging purposes")]
    //[Fact]
    public async Task ReadEditorContext()
    {
        const DbEnvironment dbEnvironment = DbEnvironment.PRD;

        using (var dbContext = GetEditorContext(dbEnvironment))
        {
            var enricher = EnrichEvent.WithTime(NodaTime.SystemClock.Instance);

            //add events
            //using (var store = GetStreamStore(dbEnvironment))
            //{
            //    IRoadNetworkEventWriter roadNetworkEventWriter = new RoadNetworkEventWriter(store, enricher);

            //    foreach (var extract in extractsToOpenAndMarkAsDownloaded)
            //    {
            //        var map = new EventSourcedEntityMap();
            //        var roadRegistryContext = new RoadRegistryContext(map,
            //            store,
            //            new FakeRoadNetworkSnapshotReader(),
            //            EventsJsonSerializerSettingsProvider.CreateSerializerSettings(),
            //            new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly)),
            //            new LoggerFactory());

            //        var extractRequest = await roadRegistryContext.RoadNetworkExtracts.Get(ExtractRequestId.FromExternalRequestId(extract.ExternalRequestId));
            //        extractRequest.Download(new DownloadId(extract.DownloadId));

            //        foreach (var entry in map.Entries)
            //        {
            //            var events = entry.Entity.TakeEvents();
            //            if (events.Length != 0)
            //            {
            //                var messageId = extract.DownloadId;
            //                await roadNetworkEventWriter.WriteAsync(entry.Stream, messageId, entry.ExpectedVersion, events, CancellationToken.None);
            //            }
            //        }
            //    }
            //}

            //load dbf records
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
        const int position = 1858902;
        var connectionString = GetEventsConnectionString(DbEnvironment.PRD);
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
