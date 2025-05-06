namespace RoadRegistry.Tests.BackOffice.Core;

using System.Data;
using System.Globalization;
using System.Text;
using Be.Vlaanderen.Basisregisters.Aws.DistributedS3Cache;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
using Editor.Schema;
using Editor.Schema.Extensions;
using FluentAssertions;
using Integration.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;
using Newtonsoft.Json;
using NodaTime;
using Producer.Snapshot.ProjectionHost.NationalRoad;
using Product.Schema;
using RoadNetwork.Schema;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Core;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.Messages;
using SqlStreamStore;
using KafkaProducer = Producer.Snapshot.ProjectionHost.Projections.KafkaProducer;

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

        var idMappings = await GenerateOrLoadNewIds(env, position);
        var europeanRoadMappings = idMappings.europeanRoadMappings;
        var nationalRoadMappings = idMappings.nationalRoadMappings;
        var numberedRoadMappings = idMappings.numberedRoadMappings;

        WriteMappingsToOutput(europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);

        await FixEditorProjections(env, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
        await FixProductProjections(env, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
        await FixProducerProjections(env, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
        await FixIntegrationProjections(env, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
        await FixWmsProjections(env, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
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

    private sealed class IdMappings
    {
        public Dictionary<int, int> europeanRoadMappings { get; set; }
        public Dictionary<int, int> nationalRoadMappings { get; set; }
        public Dictionary<int, int> numberedRoadMappings { get; set; }
    }

    private async Task<IdMappings> GenerateOrLoadNewIds(DbEnvironment environment, int position)
    {
        var cachePath = $"mappings-{environment}.json";
        if (File.Exists(cachePath))
        {
            return JsonConvert.DeserializeObject<IdMappings>(await File.ReadAllTextAsync(cachePath));
        }

        var eventsConnectionString = GetEventsConnectionString(environment);

        await using var connection = new SqlConnection(eventsConnectionString);
        await connection.OpenAsync();

        var message = await ReadMessage();

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

        var idMappings = new IdMappings
        {
            europeanRoadMappings = europeanRoadMappings,
            nationalRoadMappings = nationalRoadMappings,
            numberedRoadMappings = numberedRoadMappings
        };
        await File.WriteAllTextAsync(cachePath, JsonConvert.SerializeObject(idMappings));

        var serializedMessage = JsonConvert.SerializeObject(message, EventsJsonSerializerSettingsProvider.CreateSerializerSettings());
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"UPDATE [road-registry-events].[RoadRegistry].[Messages] SET [JsonData] = @json WHERE [Position] = {position}";
            command.Parameters.AddWithValue("json", serializedMessage);

            var executed = await command.ExecuteNonQueryAsync();
            executed.Should().Be(1);
        }

        return idMappings;

        async Task<RoadNetworkChangesAccepted> ReadMessage()
        {
            await using var command = connection.CreateCommand();
            command.CommandText = $"SELECT [JsonData] FROM [road-registry-events].[RoadRegistry].[Messages] WITH (NOLOCK) WHERE [Position] = {position}";

            await using var reader = await command.ExecuteReaderAsync(CommandBehavior.SequentialAccess);
            await reader.ReadAsync();

            Assert.True(reader.HasRows);

            var jsonData = reader.GetString(0);
            Assert.NotNull(jsonData);

            return JsonConvert.DeserializeObject<RoadNetworkChangesAccepted>(jsonData, EventsJsonSerializerSettingsProvider.CreateSerializerSettings());
        }
    }

    private async Task FixIntegrationProjections(DbEnvironment environment, Dictionary<int, int> europeanRoadMappings, Dictionary<int, int> nationalRoadMappings, Dictionary<int, int> numberedRoadMappings)
    {
        var connectionString = GetIntegrationConnectionString(environment);

        var dbContext = new IntegrationContext(new DbContextOptionsBuilder<IntegrationContext>()
            .UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions
                    .UseNetTopologySuite()
            )
            .Options);

        foreach (var mapping in europeanRoadMappings)
        {
            var item = await dbContext.RoadSegmentEuropeanRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            dbContext.RoadSegmentEuropeanRoadAttributes.Remove(item);

            dbContext.RoadSegmentEuropeanRoadAttributes.Add(new()
            {
                Id = mapping.Value,
                RoadSegmentId = item.RoadSegmentId,
                Number = item.Number,
                IsRemoved = item.IsRemoved,
                OrganizationId = item.OrganizationId,
                OrganizationName = item.OrganizationName,
                VersionTimestamp = item.VersionTimestamp,
                CreatedOnTimestamp = item.CreatedOnTimestamp,
                VersionAsString = item.VersionAsString,
                CreatedOnAsString = item.CreatedOnAsString
            });
        }

        foreach (var mapping in nationalRoadMappings)
        {
            var item = await dbContext.RoadSegmentNationalRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            dbContext.RoadSegmentNationalRoadAttributes.Remove(item);

            dbContext.RoadSegmentNationalRoadAttributes.Add(new()
            {
                Id = mapping.Value,
                RoadSegmentId = item.RoadSegmentId,
                Number = item.Number,
                IsRemoved = item.IsRemoved,
                OrganizationId = item.OrganizationId,
                OrganizationName = item.OrganizationName,
                VersionTimestamp = item.VersionTimestamp,
                CreatedOnTimestamp = item.CreatedOnTimestamp,
                VersionAsString = item.VersionAsString,
                CreatedOnAsString = item.CreatedOnAsString
            });
        }

        foreach (var mapping in numberedRoadMappings)
        {
            var item = await dbContext.RoadSegmentNumberedRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            dbContext.RoadSegmentNumberedRoadAttributes.Remove(item);

            dbContext.RoadSegmentNumberedRoadAttributes.Add(new()
            {
                Id = mapping.Value,
                RoadSegmentId = item.RoadSegmentId,
                Number = item.Number,
                IsRemoved = item.IsRemoved,
                OrganizationId = item.OrganizationId,
                OrganizationName = item.OrganizationName,
                VersionTimestamp = item.VersionTimestamp,
                CreatedOnTimestamp = item.CreatedOnTimestamp,
                VersionAsString = item.VersionAsString,
                CreatedOnAsString = item.CreatedOnAsString,
                DirectionId = item.DirectionId,
                DirectionLabel = item.DirectionLabel,
                SequenceNumber = item.SequenceNumber
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task FixWmsProjections(DbEnvironment environment, Dictionary<int, int> europeanRoadMappings, Dictionary<int, int> nationalRoadMappings, Dictionary<int, int> numberedRoadMappings)
    {
        var connectionString = GetWmsConnectionString(environment);

        var dbContext = new RoadRegistry.Wms.Schema.WmsContext(new DbContextOptionsBuilder<RoadRegistry.Wms.Schema.WmsContext>()
            .UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions
                    .UseNetTopologySuite()
            )
            .Options);

        foreach (var mapping in europeanRoadMappings)
        {
            var item = await dbContext.RoadSegmentEuropeanRoadAttributes.SingleAsync(x => x.EU_OIDN == mapping.Key);
            dbContext.RoadSegmentEuropeanRoadAttributes.Remove(item);

            dbContext.RoadSegmentEuropeanRoadAttributes.Add(new()
            {
                EU_OIDN = mapping.Value,
                WS_OIDN = item.WS_OIDN,
                BEGINORG = item.BEGINORG,
                EUNUMMER = item.EUNUMMER,
                BEGINTIJD = item.BEGINTIJD,
                LBLBGNORG = item.LBLBGNORG
            });
        }

        foreach (var mapping in nationalRoadMappings)
        {
            var item = await dbContext.RoadSegmentNationalRoadAttributes.SingleAsync(x => x.NW_OIDN == mapping.Key);
            dbContext.RoadSegmentNationalRoadAttributes.Remove(item);

            dbContext.RoadSegmentNationalRoadAttributes.Add(new()
            {
                NW_OIDN = mapping.Value,
                WS_OIDN = item.WS_OIDN,
                BEGINORG = item.BEGINORG,
                BEGINTIJD = item.BEGINTIJD,
                LBLBGNORG = item.LBLBGNORG,
                IDENT2 = item.IDENT2
            });
        }

        //n/a: numberedRoadMappings

        await dbContext.SaveChangesAsync();
    }

    private async Task FixEditorProjections(DbEnvironment environment, Dictionary<int, int> europeanRoadMappings, Dictionary<int, int> nationalRoadMappings, Dictionary<int, int> numberedRoadMappings)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

        var connectionString = GetEditorProjectionsConnectionString(environment);

        var dbContext = new EditorContext(new DbContextOptionsBuilder<EditorContext>()
            .UseSqlServer(
                connectionString,
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
            dbContext.RoadSegmentEuropeanRoadAttributes.Remove(item);

            var dbf = new RoadSegmentEuropeanRoadAttributeDbaseRecord().FromBytes(item.DbaseRecord, manager, encoding);
            dbf.EU_OIDN.Value = mapping.Value;

            dbContext.RoadSegmentEuropeanRoadAttributes.Add(new()
            {
                Id = mapping.Value,
                RoadSegmentId = item.RoadSegmentId,
                DbaseRecord = dbf.ToBytes(manager, encoding)
            });
        }

        foreach (var mapping in nationalRoadMappings)
        {
            var item = await dbContext.RoadSegmentNationalRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            dbContext.RoadSegmentNationalRoadAttributes.Remove(item);

            var dbf = new RoadSegmentNationalRoadAttributeDbaseRecord().FromBytes(item.DbaseRecord, manager, encoding);
            dbf.NW_OIDN.Value = mapping.Value;

            dbContext.RoadSegmentNationalRoadAttributes.Add(new()
            {
                Id = mapping.Value,
                RoadSegmentId = item.RoadSegmentId,
                DbaseRecord = dbf.ToBytes(manager, encoding)
            });
        }

        foreach (var mapping in numberedRoadMappings)
        {
            var item = await dbContext.RoadSegmentNumberedRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            dbContext.RoadSegmentNumberedRoadAttributes.Remove(item);

            var dbf = new RoadSegmentNumberedRoadAttributeDbaseRecord().FromBytes(item.DbaseRecord, manager, encoding);
            dbf.GW_OIDN.Value = mapping.Value;

            dbContext.RoadSegmentNumberedRoadAttributes.Add(new()
            {
                Id = mapping.Value,
                RoadSegmentId = item.RoadSegmentId,
                DbaseRecord = dbf.ToBytes(manager, encoding)
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task FixProductProjections(DbEnvironment environment, Dictionary<int, int> europeanRoadMappings, Dictionary<int, int> nationalRoadMappings, Dictionary<int, int> numberedRoadMappings)
    {
        var connectionString = GetEditorProjectionsConnectionString(environment);

        var dbContext = new ProductContext(new DbContextOptionsBuilder<ProductContext>()
            .UseSqlServer(
                connectionString,
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
            dbContext.RoadSegmentEuropeanRoadAttributes.Remove(item);

            var dbf = new RoadSegmentEuropeanRoadAttributeDbaseRecord().FromBytes(item.DbaseRecord, manager, encoding);
            dbf.EU_OIDN.Value = mapping.Value;

            dbContext.RoadSegmentEuropeanRoadAttributes.Add(new()
            {
                Id = mapping.Value,
                RoadSegmentId = item.RoadSegmentId,
                DbaseRecord = dbf.ToBytes(manager, encoding)
            });
        }

        foreach (var mapping in nationalRoadMappings)
        {
            var item = await dbContext.RoadSegmentNationalRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            dbContext.RoadSegmentNationalRoadAttributes.Remove(item);

            var dbf = new RoadSegmentNationalRoadAttributeDbaseRecord().FromBytes(item.DbaseRecord, manager, encoding);
            dbf.NW_OIDN.Value = mapping.Value;

            dbContext.RoadSegmentNationalRoadAttributes.Add(new()
            {
                Id = mapping.Value,
                RoadSegmentId = item.RoadSegmentId,
                DbaseRecord = dbf.ToBytes(manager, encoding)
            });
        }

        foreach (var mapping in numberedRoadMappings)
        {
            var item = await dbContext.RoadSegmentNumberedRoadAttributes.SingleAsync(x => x.Id == mapping.Key);
            dbContext.RoadSegmentNumberedRoadAttributes.Remove(item);

            var dbf = new RoadSegmentNumberedRoadAttributeDbaseRecord().FromBytes(item.DbaseRecord, manager, encoding);
            dbf.GW_OIDN.Value = mapping.Value;

            dbContext.RoadSegmentNumberedRoadAttributes.Add(new()
            {
                Id = mapping.Value,
                RoadSegmentId = item.RoadSegmentId,
                DbaseRecord = dbf.ToBytes(manager, encoding)
            });
        }

        await dbContext.SaveChangesAsync();
    }

    private async Task FixProducerProjections(DbEnvironment environment, Dictionary<int, int> europeanRoadMappings, Dictionary<int, int> nationalRoadMappings, Dictionary<int, int> numberedRoadMappings)
    {
        var connectionString = GetEventsConnectionString(environment);

        var dbContext = new NationalRoadProducerSnapshotContext(new DbContextOptionsBuilder<NationalRoadProducerSnapshotContext>()
            .UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions
                    .UseNetTopologySuite()
            )
            .Options);

        var kafkaProducer = new KafkaProducer(new KafkaProducerOptions(
            Configuration["Kafka:BootstrapServers"],
            Configuration[$"Kafka:SaslUserName-{environment}"],
            Configuration[$"Kafka:SaslPassword-{environment}"],
            Configuration.GetRequiredValue<string>("NationalRoadTopic"),
            true,
            EventsJsonSerializerSettingsProvider.CreateSerializerSettings()));

        foreach (var mapping in nationalRoadMappings)
        {
            var item = await dbContext.NationalRoads.SingleAsync(x => x.Id == mapping.Key);
            dbContext.NationalRoads.Remove(item);

            dbContext.NationalRoads.Add(new()
            {
                Id = mapping.Value,
                RoadSegmentId = item.RoadSegmentId,
                Number = item.Number,
                Origin = item.Origin,
                IsRemoved = item.IsRemoved,
                LastChangedTimestamp = item.LastChangedTimestamp
            });

            await Produce(item.Id, item.ToContract(), CancellationToken.None);
            await dbContext.SaveChangesAsync();
        }

        async Task Produce(int nationalRoadId, NationalRoadSnapshot snapshot, CancellationToken cancellationToken)
        {
            var result = await kafkaProducer.Produce(
                nationalRoadId.ToString(CultureInfo.InvariantCulture),
                snapshot,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
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
