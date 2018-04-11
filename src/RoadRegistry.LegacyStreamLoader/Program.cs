namespace RoadRegistry.LegacyStreamLoader
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using Aiv.Vbr.EventHandling;
    using Aiv.Vbr.Generators.Guid;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using RoadRegistry.Events;
    using SqlStreamStore;
    using SqlStreamStore.Streams;

    public class Program
    {
        private static async Task Main(string[] args)
        {
            const string LEGACY_STREAM_FILE = "LegacyStreamFile";

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{Environment.MachineName}.json", true, true)
                .AddEnvironmentVariables()
                .AddCommandLine(args);
            
            var root = configurationBuilder.Build();

            var connectionStringBuilder = new SqlConnectionStringBuilder(root.GetConnectionString("StreamStore"));
            //Attempt to reconnect every 30 seconds, for an hour - could be set in the connection string as well.
            connectionStringBuilder.ConnectRetryCount = 120;
            connectionStringBuilder.ConnectRetryInterval = 30;

            var fileSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                DateParseHandling = DateParseHandling.DateTime,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };
            var eventSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            var typeMapping = new EventMapping(
                EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly)
            );
            
            using(var streamStore = new MsSqlStreamStore(
                new MsSqlStreamStoreSettings(connectionStringBuilder.ConnectionString)))
            {
                await streamStore.CreateSchema();

                // Import the legacy stream from a json file
                var legacySteamFile = new FileInfo(root[LEGACY_STREAM_FILE]);
                var reader = new LegacyStreamFileReader(fileSettings);
                var expectedVersion = ExpectedVersion.NoStream;
                var watch = Stopwatch.StartNew();
                var index = 0;
                foreach(var batch in reader.Read(legacySteamFile).Batch(1000))
                {
                    Console.Write("Expected version is {0}.", expectedVersion);
                    Console.Write(" ");
                    watch.Restart();
                    var appendResult = await streamStore.AppendToStream(
                        new StreamId("roadnetwork"),
                        expectedVersion,
                        batch.ConvertAll(@event => new NewStreamMessage(
                            Deterministic.Create(Deterministic.Namespaces.Events, $"roadnetwork-{index++}"),
                            typeMapping.GetEventName(@event.GetType()),
                            JsonConvert.SerializeObject(@event, eventSettings),
                            JsonConvert.SerializeObject(new Dictionary<string, string>
                            {
                                { "$version", "0" }
                            }, eventSettings)
                        ))
                    );
                    Console.WriteLine("Append took {0}ms", watch.ElapsedMilliseconds);
                    expectedVersion = appendResult.CurrentVersion;
                }
            }
        }
    }
}
