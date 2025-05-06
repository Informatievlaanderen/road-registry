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
using Product.Schema;
using Product.Schema.RoadSegments;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Framework;
using RoadRegistry.BackOffice.Messages;
using Serilog.Enrichers;
using SqlStreamStore;
using RoadRegistry.BackOffice.Extensions;

public class RoadNetworkSnapshotInspectorTests
{
    public RoadNetworkSnapshotInspectorTests(IConfiguration configuration, ITestOutputHelper testOutputHelper)
    {
        Configuration = configuration;
        TestOutputHelper = testOutputHelper;
    }

    protected IConfiguration Configuration { get; }
    protected ITestOutputHelper TestOutputHelper { get; }

    private string GetEventsConnectionString(DbEnvironment environment)
    {
        return Configuration.GetConnectionString($"Events-{environment}") ?? Configuration.GetRequiredConnectionString("Events");
    }
    private string GetEditorProjectionsConnectionString(DbEnvironment environment)
    {
        return Configuration.GetConnectionString($"EditorProjections-{environment}") ?? Configuration.GetRequiredConnectionString("EditorProjections");
    }

    private IStreamStore GetStreamStore(DbEnvironment dbEnvironment)
    {
        var connectionString = GetEventsConnectionString(dbEnvironment);
        return new MsSqlStreamStoreV3(
            new MsSqlStreamStoreV3Settings(
                connectionString)
            {
                Schema = WellKnownSchemas.EventSchema
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
    private ProductContext GetProductContext(DbEnvironment dbEnvironment)
    {
        return new ProductContext(new DbContextOptionsBuilder<ProductContext>()
                   //.UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll)
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

    [Fact(Skip = "Reads data from ProductContext. Useful for debugging purposes")]
    //[Fact]
    public async Task ReadProductContext()
    {
        const DbEnvironment dbEnvironment = DbEnvironment.DEV;

        using (var dbContext = GetProductContext(dbEnvironment))
        {
            //dbContext.ChangeTracker.AutoDetectChangesEnabled = false;

            //var dbRecord = await dbContext.RoadSegments.IncludeLocalSingleOrDefaultAsync(x => x.Id == 9999999, CancellationToken.None).ConfigureAwait(false);
            //if (dbRecord is not null)
            //{
            //    dbContext.RoadSegments.Remove(dbRecord);
            //}

            //var dbRecord2 = await dbContext.RoadSegments.IncludeLocalSingleOrDefaultAsync(x => x.Id == 9999999, CancellationToken.None).ConfigureAwait(false);

            //dbContext.RoadSegments.Add(new RoadSegmentRecord
            //{
            //    Id = 9999999,
            //    DbaseRecord = Array.Empty<byte>(),
            //    ShapeRecordContent = Array.Empty<byte>()
            //});

            //dbContext.SaveChanges();
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
        const string tempFilePath = @"snapshot";

        var snapshotBytes = await File.ReadAllBytesAsync(tempFilePath);
        var snapshot = S3CacheSerializer.Serializer.DeserializeObject<RoadNetworkSnapshot>(snapshotBytes, true).Value;

        //FindIdenticalRoadSegmentsForFakeNodeConnections(snapshot);
    }

    private void FindIdenticalRoadSegmentsForFakeNodeConnections(RoadNetworkSnapshot snapshot)
    {
        var fakeRoadNodes = snapshot.Nodes
            .Where(x => x.Type == RoadNodeType.FakeNode)
            .ToArray();
        var fakeRoadNodeIds = fakeRoadNodes.Select(x => x.Id).ToArray();
        var roadSegments = snapshot.Segments
            .Where(x => fakeRoadNodeIds.Contains(x.StartNodeId) || fakeRoadNodeIds.Contains(x.EndNodeId))
            .ToArray();

        var counter = 0;

        TestOutputHelper.WriteLine("RoadNodeId;Segment1;Segment2;LocationX;LocationY");
        foreach (var roadNode in fakeRoadNodes)
        {
            var roadNodeId = roadNode.Id;
            var nodeSegments = roadSegments
                .Where(x => x.StartNodeId == roadNodeId || x.EndNodeId == roadNodeId)
                .ToArray();

            if (nodeSegments.Length != 2)
            {
                continue;
            }

            var segment1 = nodeSegments[0];
            var segment2 = nodeSegments[1];

            var attributeHash1 = new AttributeHash(
                RoadSegmentAccessRestriction.Parse(segment1.AttributeHash.AccessRestriction),
                RoadSegmentCategory.Parse(segment1.AttributeHash.Category),
                RoadSegmentMorphology.Parse(segment1.AttributeHash.Morphology),
                RoadSegmentStatus.Parse(segment1.AttributeHash.Status),
                StreetNameLocalId.FromValue(segment1.AttributeHash.LeftSideStreetNameId),
                StreetNameLocalId.FromValue(segment1.AttributeHash.RightSideStreetNameId),
                new OrganizationId(segment1.AttributeHash.OrganizationId),
                RoadSegmentGeometryDrawMethod.Parse(segment1.AttributeHash.GeometryDrawMethod)
            );
            var attributeHash2 = new AttributeHash(
                RoadSegmentAccessRestriction.Parse(segment2.AttributeHash.AccessRestriction),
                RoadSegmentCategory.Parse(segment2.AttributeHash.Category),
                RoadSegmentMorphology.Parse(segment2.AttributeHash.Morphology),
                RoadSegmentStatus.Parse(segment2.AttributeHash.Status),
                StreetNameLocalId.FromValue(segment2.AttributeHash.LeftSideStreetNameId),
                StreetNameLocalId.FromValue(segment2.AttributeHash.RightSideStreetNameId),
                new OrganizationId(segment2.AttributeHash.OrganizationId),
                RoadSegmentGeometryDrawMethod.Parse(segment2.AttributeHash.GeometryDrawMethod)
            );
            var segmentsEqualAttributes = attributeHash1.Equals(attributeHash2);
            if (segmentsEqualAttributes)
            {
                TestOutputHelper.WriteLine($"{roadNode.Id};{segment1.Id};{segment2.Id};{roadNode.Geometry.Point.X};{roadNode.Geometry.Point.Y}");
                counter++;
            }
        }
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
