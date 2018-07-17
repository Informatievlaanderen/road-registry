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
    using Events;
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using System.Linq;
    using System.Collections.Concurrent;

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

            // Attempt to reconnect every 30 seconds, for an hour - could be set in the connection string as well.
            var connectionStringBuilder =
                new SqlConnectionStringBuilder(root.GetConnectionString("StreamStore"))
                {
                    ConnectRetryCount = 120,
                    ConnectRetryInterval = 30
                };

            var fileSettings = new JsonSerializerSettings()
            {
                Formatting = Formatting.Indented,
                DateFormatHandling = DateFormatHandling.IsoDateFormat,
                DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                DateParseHandling = DateParseHandling.DateTime,
                DefaultValueHandling = DefaultValueHandling.Ignore
            };

            var eventSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            var typeMapping = new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));

            using (var streamStore = new MsSqlStreamStore(new MsSqlStreamStoreSettings(connectionStringBuilder.ConnectionString)
            {
                Schema = "RoadRegistry"
            }))
            {
                await streamStore.CreateSchema();

                // Import the legacy stream from a json file
                var legacyStreamFilePath = root[LEGACY_STREAM_FILE];
                Console.WriteLine($"Importing from {legacyStreamFilePath}");
                var legacyStreamFile = new FileInfo(legacyStreamFilePath);
                var reader = new LegacyStreamFileReader(fileSettings);

                var expectedVersions = new ConcurrentDictionary<string, int>();
                var innerWatch = Stopwatch.StartNew();
                var outerWatch = Stopwatch.StartNew();

                foreach (var batch in reader.Read(legacyStreamFile).Batch(1000))
                {
                    // foreach(var item in batch)
                    // {
                    //     Console.WriteLine("{0}:{1}", item.Stream, item.Event.GetType().Name);
                    // }

                    foreach(var stream in batch.GroupBy(item => item.Stream, item => item.Event))
                    {
                        int expectedVersion;
                        if(!expectedVersions.TryGetValue(stream.Key, out expectedVersion))
                        {
                            expectedVersion = ExpectedVersion.NoStream;
                        }

                        innerWatch.Restart();

                        var appendResult = await streamStore.AppendToStream(
                            new StreamId(stream.Key),
                            expectedVersion,
                            stream
                                .Select(@event => new NewStreamMessage(
                                    Deterministic.Create(Deterministic.Namespaces.Events, $"{stream.Key}-{expectedVersion++}"),
                                    typeMapping.GetEventName(@event.GetType()),
                                    JsonConvert.SerializeObject(@event, eventSettings),
                                    JsonConvert.SerializeObject(new Dictionary<string, string>
                                    {
                                        { "$version", "0" }
                                    }, eventSettings)
                                ))
                                .ToArray()
                        );

                        Console.WriteLine("Append took {0}ms for stream {1}@{2}",
                            innerWatch.ElapsedMilliseconds,
                            stream.Key,
                            appendResult.CurrentVersion);

                        expectedVersions[stream.Key] = appendResult.CurrentVersion;
                    }
                }
                Console.WriteLine("Total append took {0}ms", outerWatch.ElapsedMilliseconds);
            }
        }
    }
}
