namespace RoadRegistry.Tests.BackOffice.Core;

using System.Data;
using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Editor.Schema;
using Editor.Schema.Extensions;
using Integration.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;
using Newtonsoft.Json;
using NodaTime;
using Product.Schema;
using RoadNetwork.Schema;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.Messages;
using SqlStreamStore;

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
    private string GetIntegrationConnectionString(DbEnvironment environment)
    {
        return Configuration.GetConnectionString($"Integration-{environment}") ?? Configuration.GetRequiredConnectionString("Integration");
    }
    private string GetWmsConnectionString(DbEnvironment environment)
    {
        return Configuration.GetConnectionString($"Wms-{environment}") ?? Configuration.GetRequiredConnectionString("Wms");
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
            var enricher = EnrichEvent.WithTime(SystemClock.Instance);

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

    //[Fact(Skip = "Loads a message to your local computer. Useful for debugging purposes")]
    [Fact]
    public async Task InspectMessage()
    {
        const DbEnvironment env = DbEnvironment.STG;
        const int position = 2252414;
        var connectionString = GetEventsConnectionString(env);
        var messageFilePath = $"message-{env}-{position}.json";

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

    [Fact]
    public async Task GenerateNewAttributeIdsForNumbersInEvent()
    {
        const DbEnvironment env = DbEnvironment.STG;
        const int position = 2252414;

        var eventsConnectionString = GetEventsConnectionString(env);
        var projectionsConnectionString = GetEditorProjectionsConnectionString(env);
        var integrationConnectionString = GetIntegrationConnectionString(env);
        var wmsConnectionString = GetWmsConnectionString(env);

        await using (var connection = new SqlConnection(eventsConnectionString))
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

                var message = JsonConvert.DeserializeObject<RoadNetworkChangesAccepted>(jsonData, EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                var (europeanRoadMappings, nationalRoadMappings, numberedRoadMappings) = await GenerateNewIds(eventsConnectionString, message);
                WriteMappingsToOutput(europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);

                await UpdateEventInDb(message, command, position);

                await FixEditorProjections(projectionsConnectionString, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
                await FixProductProjections(projectionsConnectionString, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
                await FixIntegrationProjections(integrationConnectionString, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
                await FixWmsProjections(wmsConnectionString, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
            }
        }
    }

    private static async Task UpdateEventInDb(RoadNetworkChangesAccepted message, SqlCommand command, int position)
    {
        var serializeMessage = JsonConvert.SerializeObject(message, EventsJsonSerializerSettingsProvider.CreateSerializerSettings());
        command.CommandText = $"UPDATE [JsonData] = @json FROM [road-registry-events].[RoadRegistry].[Messages] WHERE [Position] = {position}";
        command.Parameters.AddWithValue("json", serializeMessage);
        var executed = await command.ExecuteNonQueryAsync();
    }

    private void WriteMappingsToOutput(Dictionary<int, int> europeanRoadMappings, Dictionary<int, int> nationalRoadMappings, Dictionary<int, int> numberedRoadMappings)
    {
        TestOutputHelper.WriteLine("Europese weg attributen: ");
        foreach (var mapping in europeanRoadMappings)
        {
            TestOutputHelper.WriteLine($"{mapping.Key} -> {mapping.Value}");
        }
        TestOutputHelper.WriteLine("");
        TestOutputHelper.WriteLine("Nationale weg attributen: ");
        foreach (var mapping in nationalRoadMappings)
        {
            TestOutputHelper.WriteLine($"{mapping.Key} -> {mapping.Value}");
        }
        TestOutputHelper.WriteLine("");
        TestOutputHelper.WriteLine("Genummerde weg attributen: ");
        foreach (var mapping in numberedRoadMappings)
        {
            TestOutputHelper.WriteLine($"{mapping.Key} -> {mapping.Value}");
        }
        TestOutputHelper.WriteLine("");
    }

    private async Task<(Dictionary<int, int> europeanRoadMappings, Dictionary<int, int> nationalRoadMappings, Dictionary<int, int> numberedRoadMappings)> GenerateNewIds(string eventsConnectionString, RoadNetworkChangesAccepted message)
    {
        var roadNetworkDbIdGenerator = new RoadNetworkDbIdGenerator(new RoadNetworkDbContext(new DbContextOptionsBuilder<RoadNetworkDbContext>()
            .UseSqlServer(
                eventsConnectionString,
                sqlOptions => sqlOptions
                    .UseNetTopologySuite()
            )
            .Options));

        var europeanRoadMappings = new Dictionary<int, int>();
        var nationalRoadMappings = new Dictionary<int, int>();
        var numberedRoadMappings = new Dictionary<int, int>();

        // fix attribute_id in event (keep mapping van old->new)
        foreach (var change in message.Changes.Flatten().OfType<RoadSegmentAddedToEuropeanRoad>())
        {
            var newId = await roadNetworkDbIdGenerator.NewEuropeanRoadAttributeId();
            var oldId = change.AttributeId;
            europeanRoadMappings.Add(oldId, newId);

            change.AttributeId = newId;
        }

        foreach (var change in message.Changes.Flatten().OfType<RoadSegmentAddedToNationalRoad>())
        {
            var newId = await roadNetworkDbIdGenerator.NewNationalRoadAttributeId();
            var oldId = change.AttributeId;
            nationalRoadMappings.Add(oldId, newId);

            change.AttributeId = newId;
        }

        foreach (var change in message.Changes.Flatten().OfType<RoadSegmentAddedToNumberedRoad>())
        {
            var newId = await roadNetworkDbIdGenerator.NewNumberedRoadAttributeId();
            var oldId = change.AttributeId;
            numberedRoadMappings.Add(oldId, newId);

            change.AttributeId = newId;
        }

        return (europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
    }

    private static async Task FixIntegrationProjections(string integrationConnectionString, Dictionary<int, int> europeanRoadMappings, Dictionary<int, int> nationalRoadMappings, Dictionary<int, int> numberedRoadMappings)
    {
        var dbContext = new IntegrationContext(new DbContextOptionsBuilder<IntegrationContext>()
            .UseSqlServer(
                integrationConnectionString,
                sqlOptions => sqlOptions
                    .UseNetTopologySuite()
            )
            .Options);

        foreach (var mapping in europeanRoadMappings)
        {
            var item = await dbContext.RoadSegmentEuropeanRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            item.Id = mapping.Value;
        }

        foreach (var mapping in nationalRoadMappings)
        {
            var item = await dbContext.RoadSegmentNationalRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            item.Id = mapping.Value;
        }

        foreach (var mapping in numberedRoadMappings)
        {
            var item = await dbContext.RoadSegmentNumberedRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            item.Id = mapping.Value;
        }
        await dbContext.SaveChangesAsync();
    }

    private static async Task FixWmsProjections(string wmsConnectionString, Dictionary<int, int> europeanRoadMappings, Dictionary<int, int> nationalRoadMappings, Dictionary<int, int> numberedRoadMappings)
    {
        var dbContext = new RoadRegistry.Wms.Schema.WmsContext(new DbContextOptionsBuilder<RoadRegistry.Wms.Schema.WmsContext>()
            .UseSqlServer(
                wmsConnectionString,
                sqlOptions => sqlOptions
                    .UseNetTopologySuite()
            )
            .Options);

        foreach (var mapping in europeanRoadMappings)
        {
            var item = await dbContext.RoadSegmentEuropeanRoadAttributes.SingleAsync(x => x.EU_OIDN == mapping.Key);
            item.EU_OIDN = mapping.Value;
        }

        foreach (var mapping in nationalRoadMappings)
        {
            var item = await dbContext.RoadSegmentNationalRoadAttributes.SingleAsync(x => x.NW_OIDN == mapping.Key);
            item.NW_OIDN = mapping.Value;
        }

        //n/a: numberedRoadMappings

        await dbContext.SaveChangesAsync();
    }

    private static async Task FixEditorProjections(string projectionsConnectionString, Dictionary<int, int> europeanRoadMappings, Dictionary<int, int> nationalRoadMappings, Dictionary<int, int> numberedRoadMappings)
    {
        var dbContext = new EditorContext(new DbContextOptionsBuilder<EditorContext>()
            .UseSqlServer(
                projectionsConnectionString,
                sqlOptions => sqlOptions
                    .UseNetTopologySuite()
            )
            .Options);

        var manager = new RecyclableMemoryStreamManager();
        var encoding = WellKnownEncodings.WindowsAnsi;

        //  fix DBF vanuit lokaal, delete record van old attribute_id, add new met new attribute_id
        foreach (var mapping in europeanRoadMappings)
        {
            var item = await dbContext.RoadSegmentEuropeanRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            item.Id = mapping.Value;

            var dbf = new RoadSegmentEuropeanRoadAttributeDbaseRecord().FromBytes(item.DbaseRecord, manager, encoding);
            dbf.EU_OIDN.Value = mapping.Value;
            item.DbaseRecord = dbf.ToBytes(manager, encoding);
        }

        foreach (var mapping in nationalRoadMappings)
        {
            var item = await dbContext.RoadSegmentNationalRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            item.Id = mapping.Value;

            var dbf = new RoadSegmentNationalRoadAttributeDbaseRecord().FromBytes(item.DbaseRecord, manager, encoding);
            dbf.NW_OIDN.Value = mapping.Value;
            item.DbaseRecord = dbf.ToBytes(manager, encoding);
        }

        foreach (var mapping in numberedRoadMappings)
        {
            var item = await dbContext.RoadSegmentNumberedRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            item.Id = mapping.Value;

            var dbf = new RoadSegmentNumberedRoadAttributeDbaseRecord().FromBytes(item.DbaseRecord, manager, encoding);
            dbf.GW_OIDN.Value = mapping.Value;
            item.DbaseRecord = dbf.ToBytes(manager, encoding);
        }

        await dbContext.SaveChangesAsync();
    }

    private static async Task FixProductProjections(string projectionsConnectionString, Dictionary<int, int> europeanRoadMappings, Dictionary<int, int> nationalRoadMappings, Dictionary<int, int> numberedRoadMappings)
    {
        var dbContext = new ProductContext(new DbContextOptionsBuilder<ProductContext>()
            .UseSqlServer(
                projectionsConnectionString,
                sqlOptions => sqlOptions
                    .UseNetTopologySuite()
            )
            .Options);

        var manager = new RecyclableMemoryStreamManager();
        var encoding = WellKnownEncodings.WindowsAnsi;

        //  fix DBF vanuit lokaal, delete record van old attribute_id, add new met new attribute_id
        foreach (var mapping in europeanRoadMappings)
        {
            var item = await dbContext.RoadSegmentEuropeanRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            item.Id = mapping.Value;

            var dbf = new RoadSegmentEuropeanRoadAttributeDbaseRecord().FromBytes(item.DbaseRecord, manager, encoding);
            dbf.EU_OIDN.Value = mapping.Value;
            item.DbaseRecord = dbf.ToBytes(manager, encoding);
        }

        foreach (var mapping in nationalRoadMappings)
        {
            var item = await dbContext.RoadSegmentNationalRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            item.Id = mapping.Value;

            var dbf = new RoadSegmentNationalRoadAttributeDbaseRecord().FromBytes(item.DbaseRecord, manager, encoding);
            dbf.NW_OIDN.Value = mapping.Value;
            item.DbaseRecord = dbf.ToBytes(manager, encoding);
        }

        foreach (var mapping in numberedRoadMappings)
        {
            var item = await dbContext.RoadSegmentNumberedRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            item.Id = mapping.Value;

            var dbf = new RoadSegmentNumberedRoadAttributeDbaseRecord().FromBytes(item.DbaseRecord, manager, encoding);
            dbf.GW_OIDN.Value = mapping.Value;
            item.DbaseRecord = dbf.ToBytes(manager, encoding);
        }

        await dbContext.SaveChangesAsync();
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
