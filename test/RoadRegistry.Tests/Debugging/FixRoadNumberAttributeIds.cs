namespace RoadRegistry.Tests.Debugging;

using System.Data;
using System.Text;
using Be.Vlaanderen.Basisregisters.EventHandling;
using Be.Vlaanderen.Basisregisters.GrAr.Contracts.RoadRegistry;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
using Editor.Schema;
using Editor.Schema.Extensions;
using FluentAssertions;
using Integration.Schema;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IO;
using Newtonsoft.Json;
using Producer.Snapshot.ProjectionHost.NationalRoad;
using Producer.Snapshot.ProjectionHost.Shared;
using Product.Schema;
using RoadNetwork.Schema;
using RoadRegistry.BackOffice;
using RoadRegistry.BackOffice.Extracts.Dbase.RoadSegments;
using RoadRegistry.BackOffice.Messages;
using RoadRegistry.Wms.Schema;

public class FixRoadNumberAttributeIds
{
    public FixRoadNumberAttributeIds(IConfiguration configuration, ITestOutputHelper testOutputHelper)
    {
        Configuration = configuration;
        TestOutputHelper = testOutputHelper;
    }

    private IConfiguration Configuration { get; }
    private ITestOutputHelper TestOutputHelper { get; }

    //[Fact]
    [Fact(Skip = "For debugging purposes only")]
    public async Task Run()
    {
        const DbEnvironment env = DbEnvironment.STG;
        const int position = 2252414;
        // const DbEnvironment env = DbEnvironment.PRD;
        // const int position = 2273541;

        var idMappings = await GenerateOrLoadNewIds(env, position);
        var europeanRoadMappings = idMappings.europeanRoadMappings;
        var nationalRoadMappings = idMappings.nationalRoadMappings;
        var numberedRoadMappings = idMappings.numberedRoadMappings;

        WriteMappingsToOutput(europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);

        await FixWmsProjections(env, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
        await FixEditorProjections(env, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
        await FixProductProjections(env, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
        await FixProducerProjections(env, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
        await FixIntegrationProjections(env, europeanRoadMappings, nationalRoadMappings, numberedRoadMappings);
    }

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
            TestOutputHelper.WriteLine($"Loading mappings from cache: {cachePath}");
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
            var newId = await roadNetworkDbIdGenerator.NewEuropeanRoadAttributeIdAsync();
            var oldId = change.AttributeId;
            europeanRoadMappings.Add(oldId, newId);

            change.AttributeId = newId;
        }

        foreach (var change in message.Changes.Flatten().OfType<RoadSegmentAddedToNationalRoad>())
        {
            var newId = await roadNetworkDbIdGenerator.NewNationalRoadAttributeIdAsync();
            var oldId = change.AttributeId;
            nationalRoadMappings.Add(oldId, newId);

            change.AttributeId = newId;
        }

        foreach (var change in message.Changes.Flatten().OfType<RoadSegmentAddedToNumberedRoad>())
        {
            var newId = await roadNetworkDbIdGenerator.NewNumberedRoadAttributeIdAsync();
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

            await File.WriteAllTextAsync($"message-{environment}-{position}.json",jsonData);

            return JsonConvert.DeserializeObject<RoadNetworkChangesAccepted>(jsonData, EventsJsonSerializerSettingsProvider.CreateSerializerSettings());
        }
    }

    private async Task FixIntegrationProjections(DbEnvironment environment, Dictionary<int, int> europeanRoadMappings, Dictionary<int, int> nationalRoadMappings, Dictionary<int, int> numberedRoadMappings)
    {
        var connectionString = GetIntegrationConnectionString(environment);

        var dbContext = new IntegrationContext(new DbContextOptionsBuilder<IntegrationContext>()
            .UseNpgsql(
                connectionString,
                sqlOptions => sqlOptions
                    .UseNetTopologySuite()
            )
            .Options);

        foreach (var mapping in europeanRoadMappings)
        {
            var item = await dbContext.RoadSegmentEuropeanRoadAttributes.SingleOrDefaultAsync(x => x.Id == mapping.Key);
            if (item is null)
            {
                TestOutputHelper.WriteLine($"Integration.european: no record found for {mapping.Key}");
                continue;
            }
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
            var item = await dbContext.RoadSegmentNationalRoadAttributes.SingleOrDefaultAsync(x => x.Id == mapping.Key);
            if (item is null)
            {
                TestOutputHelper.WriteLine($"Integration.national: no record found for {mapping.Key}");
                continue;
            }
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
            var item = await dbContext.RoadSegmentNumberedRoadAttributes.SingleOrDefaultAsync(x => x.Id == mapping.Key);
            if (item is null)
            {
                TestOutputHelper.WriteLine($"Integration.numbered: no record found for {mapping.Key}");
                continue;
            }
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

        var dbContext = new WmsContext(new DbContextOptionsBuilder<WmsContext>()
            .UseSqlServer(
                connectionString,
                sqlOptions => sqlOptions
                    .UseNetTopologySuite()
            )
            .Options);

        foreach (var mapping in europeanRoadMappings)
        {
            var item = await dbContext.RoadSegmentEuropeanRoadAttributes.SingleOrDefaultAsync(x => x.EU_OIDN == mapping.Key);
            if (item is null)
            {
                TestOutputHelper.WriteLine($"Wms.european: no record found for {mapping.Key}");
                continue;
            }
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
            var item = await dbContext.RoadSegmentNationalRoadAttributes.SingleOrDefaultAsync(x => x.NW_OIDN == mapping.Key);
            if (item is null)
            {
                TestOutputHelper.WriteLine($"Wms.national: no record found for {mapping.Key}");
                continue;
            }
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
            var item = await dbContext.RoadSegmentEuropeanRoadAttributes.SingleOrDefaultAsync(x => x.Id == mapping.Key);
            if (item is null)
            {
                TestOutputHelper.WriteLine($"Editor.european: no record found for {mapping.Key}");
                continue;
            }
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
            var item = await dbContext.RoadSegmentNationalRoadAttributes.SingleOrDefaultAsync(x => x.Id == mapping.Key);
            if (item is null)
            {
                TestOutputHelper.WriteLine($"Editor.national: no record found for {mapping.Key}");
                continue;
            }
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
            var item = await dbContext.RoadSegmentNumberedRoadAttributes.SingleOrDefaultAsync(x => x.Id == mapping.Key);
            if (item is null)
            {
                TestOutputHelper.WriteLine($"Editor.numbered: no record found for {mapping.Key}");
                continue;
            }
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
            var item = await dbContext.RoadSegmentEuropeanRoadAttributes.SingleOrDefaultAsync(x => x.Id == mapping.Key);
            if (item is null)
            {
                TestOutputHelper.WriteLine($"Product.european: no record found for {mapping.Key}");
                continue;
            }
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
            var item = await dbContext.RoadSegmentNationalRoadAttributes.SingleOrDefaultAsync(x => x.Id == mapping.Key);
            if (item is null)
            {
                TestOutputHelper.WriteLine($"Product.national: no record found for {mapping.Key}");
                continue;
            }
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
            var item = await dbContext.RoadSegmentNumberedRoadAttributes.SingleOrDefaultAsync(x => x.Id == mapping.Key);
            if (item is null)
            {
                TestOutputHelper.WriteLine($"Product.numbered: no record found for {mapping.Key}");
                continue;
            }
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

        var producerOptions = new ProducerOptions(
            new BootstrapServers(Configuration.GetRequiredValue<string>("Kafka:BootstrapServers")),
            new Topic(Configuration.GetRequiredValue<string>("NationalRoadTopic")),
            useSinglePartition: true,
            jsonSerializerSettings: EventsJsonSerializerSettingsProvider.CreateSerializerSettings()
        );
        var saslUsername = Configuration[$"Kafka:SaslUserName-{environment}"];
        var saslPassword = Configuration[$"Kafka:SaslPassword-{environment}"];
        if (!string.IsNullOrEmpty(saslUsername) && !string.IsNullOrEmpty(saslPassword))
        {
            producerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                saslUsername,
                saslPassword));
        }
        var nationalRoadKafkaProducer = new KafkaProducer(producerOptions);

        foreach (var mapping in nationalRoadMappings)
        {
            var item = await dbContext.NationalRoads.SingleOrDefaultAsync(x => x.Id == mapping.Key);
            if (item is null)
            {
                TestOutputHelper.WriteLine($"Producer.national: no record found for {mapping.Key}");
                continue;
            }
            dbContext.NationalRoads.Remove(item);

            dbContext.NationalRoads.Add(new()
            {
                Id = mapping.Value,
                RoadSegmentId = item.RoadSegmentId,
                Number = item.Number,
                Origin = new()
                {
                    Timestamp = item.Origin.Timestamp,
                    Organization = item.Origin.Organization
                },
                IsRemoved = item.IsRemoved,
                LastChangedTimestamp = item.LastChangedTimestamp
            });

            await Produce(item.Id, item.ToContract(), CancellationToken.None);
            await dbContext.SaveChangesAsync();
        }

        async Task Produce(int nationalRoadId, NationalRoadSnapshot snapshot, CancellationToken cancellationToken)
        {
            var result = await nationalRoadKafkaProducer.Produce(
                nationalRoadId,
                snapshot,
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason);
            }
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
