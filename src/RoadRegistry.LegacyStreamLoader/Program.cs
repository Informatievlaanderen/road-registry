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
    using System.Threading;
    using Amazon.S3;
    using Amazon.S3.Model;


    public class Program
    {
        const string USE_LOCAL_FILE = "UseLocalFile";
        const string LEGACY_STREAM_FILE_NAME = "LegacyStreamFileName";
        const string LEGACY_STREAM_FILE_BUCKET = "LegacyStreamFileBucket";
        const string LEGACY_STREAM_FILE_DIRECTORY = "LegacyStreamDirectory";

        private static readonly StreamId _legacyImportStream = new StreamId("legacy-roadnetwork-import");


        private static async Task Main(string[] args)
        {
            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, true)
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

            using (var streamStore = new MsSqlStreamStore(new MsSqlStreamStoreSettings(connectionStringBuilder.ConnectionString)
            {
                Schema = "RoadRegistry"
            }))
            {
                await streamStore.CreateSchema();

                var legacyImportStreamMetaData = await streamStore.GetStreamMetadata(_legacyImportStream);
                if (legacyImportStreamMetaData.MetadataStreamVersion == ExpectedVersion.NoStream)
                    await ImportStreams(root, streamStore);
                else
                    Console.WriteLine("Legacy streams already imported. Aborted import");
            }
        }

        private static async Task ImportStreams(IConfiguration root, MsSqlStreamStore streamStore)
        {
            var eventSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            var typeMapping = new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));
            var reader = new LegacyStreamFileReader(
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                    DateParseHandling = DateParseHandling.DateTime,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                }
            );

            var expectedVersions = new ConcurrentDictionary<StreamId, int>();
            var innerWatch = Stopwatch.StartNew();
            var outerWatch = Stopwatch.StartNew();

            var getEventStream = GetLegacyEventStream(root);
            if (null != getEventStream)
            {
                await appendEventStream(
                    _legacyImportStream,
                    new[] { new ImportLegacyRegistryStarted { StartedAt = DateTime.UtcNow } },
                    streamStore,
                    eventSettings,
                    typeMapping,
                    expectedVersions,
                    innerWatch
                );

                foreach (var batch in reader.Read(getEventStream).Batch(1000))
                {
                    foreach (var stream in batch.GroupBy(item => item.Stream, item => item.Event))
                    {
                        await appendEventStream(
                            new StreamId(stream.Key),
                            stream,
                            streamStore,
                            eventSettings,
                            typeMapping,
                            expectedVersions,
                            innerWatch
                        );
                    }
                }

                await appendEventStream(
                    _legacyImportStream,
                    new[] { new ImportLegacyRegistryFinished { FinishedAt = DateTime.UtcNow } },
                    streamStore,
                    eventSettings,
                    typeMapping,
                    expectedVersions,
                    innerWatch
                );

                Console.WriteLine("Total append took {0}ms", outerWatch.ElapsedMilliseconds);
            }
            else
            {
                Console.WriteLine("No event stream found");
            }
        }

        private static async Task appendEventStream(
            StreamId streamId,
            IEnumerable<object> stream,
            MsSqlStreamStore streamStore,
            JsonSerializerSettings eventSettings,
            EventMapping typeMapping,
            ConcurrentDictionary<StreamId, int> expectedVersions,
            Stopwatch watch
        )
        {
            if (!expectedVersions.TryGetValue(streamId, out var expectedVersion))
            {
                expectedVersion = ExpectedVersion.NoStream;
            }

            watch.Restart();

            var appendResult = await streamStore.AppendToStream(
                streamId,
                expectedVersion,
                stream
                    .Select(@event => new NewStreamMessage(
                        Deterministic.Create(Deterministic.Namespaces.Events,$"{streamId}-{expectedVersion++}"),
                        typeMapping.GetEventName(@event.GetType()),
                        JsonConvert.SerializeObject(@event, eventSettings),
                        JsonConvert.SerializeObject(new Dictionary<string, string>{ {"$version", "0"} }, eventSettings)
                    ))
                    .ToArray()
            );

            Console.WriteLine("Append took {0}ms for stream {1}@{2}",
                watch.ElapsedMilliseconds,
                streamId,
                appendResult.CurrentVersion);

            expectedVersions[streamId] = appendResult.CurrentVersion;
        }

        private static Func<Stream> GetLegacyEventStream(IConfiguration root)
        {
            return root.GetValue<bool>(USE_LOCAL_FILE)
                ? GetLocalLegacyStream(root)
                : GetS3LegacyStream(root);
        }

        private static Func<Stream> GetS3LegacyStream(IConfiguration root)
        {
            var bucketName = root[LEGACY_STREAM_FILE_BUCKET];
            var file = root[LEGACY_STREAM_FILE_NAME];

            try
            {
                var s3Client = root
                    .GetAWSOptions()
                    .CreateServiceClient<IAmazonS3>();

                var s3Files = s3Client
                    .ListObjectsAsync(
                        new ListObjectsRequest { BucketName = bucketName },
                        CancellationToken.None
                    ).GetAwaiter().GetResult() // ToDo: change to asynchronous if it's worth the ripple in the code
                    .S3Objects;

                if (s3Files.Any(s3File => s3File.Key == file))
                {
                    return () =>
                    {
                        Console.WriteLine($"Start download S3:{bucketName}/{file} ...");
                        return s3Client.GetObjectStreamAsync(
                            bucketName,
                            file,
                            new Dictionary<string, object>(),
                            CancellationToken.None
                        ).GetAwaiter().GetResult(); // ToDo: change to asynchronous if it's worth the ripple in the code
                    };
                }

                Console.WriteLine($"File S3:{bucketName}/{file} does not exist");
            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error downlading S3:{bucketName}/{file}: {exception}");
            }

            return null;
        }

        private static Func<Stream> GetLocalLegacyStream(IConfiguration root)
        {
            var directory = root[LEGACY_STREAM_FILE_DIRECTORY] ?? string.Empty;
            var fileName = root[LEGACY_STREAM_FILE_NAME] ?? string.Empty;
            var filePath = Path.Combine(directory, fileName);

            try
            {
                var legacyStreamsArchiveInfo = new FileInfo(filePath);
                if (legacyStreamsArchiveInfo.Exists)
                {
                    return () =>
                    {
                        Console.WriteLine($"Open {legacyStreamsArchiveInfo.FullName}");
                        return legacyStreamsArchiveInfo.OpenRead();
                    };
                }

                Console.WriteLine($"Import file '{legacyStreamsArchiveInfo.FullName}' does not exist");
            }
            catch
            {
                Console.WriteLine($"Import file path '{filePath}' is not valid");
            }

            return null;
        }
    }
}
