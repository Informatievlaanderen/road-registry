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

                var legacyStreamsArchiveInfo = GetLegacyStreamsArchiveInfo(root);
                await ImportStreams(legacyStreamsArchiveInfo, streamStore, root);
            }
        }

        private static async Task ImportStreams(FileInfo legacyStreamArchive, MsSqlStreamStore streamStore, IConfigurationRoot root)
        {
            var eventSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            var typeMapping =
                new EventMapping(EventMapping.DiscoverEventNamesInAssembly(typeof(RoadNetworkEvents).Assembly));
            var reader = new LegacyStreamFileReader(
                new JsonSerializerSettings
                {
                    Formatting = Formatting.Indented,
                    DateFormatHandling = DateFormatHandling.IsoDateFormat,
                    DateTimeZoneHandling = DateTimeZoneHandling.Unspecified,
                    DateParseHandling = DateParseHandling.DateTime,
                    DefaultValueHandling = DefaultValueHandling.Ignore
                },
                Console.WriteLine);

            var expectedVersions = new ConcurrentDictionary<string, int>();
            var innerWatch = Stopwatch.StartNew();
            var outerWatch = Stopwatch.StartNew();

            var events = UseLocalLegacyStream(root)
                ? reader.Read(GetLegacyStreamsArchiveInfo(root))
                : reader.Read(await GetS3LegacyStreamArchive(root));
            foreach (var batch in events.Batch(1000))
            {
                foreach (var stream in batch.GroupBy(item => item.Stream, item => item.Event))
                {
                    int expectedVersion;
                    if (!expectedVersions.TryGetValue(stream.Key, out expectedVersion))
                    {
                        expectedVersion = ExpectedVersion.NoStream;
                    }

                    innerWatch.Restart();

                    var appendResult = await streamStore.AppendToStream(
                        new StreamId(stream.Key),
                        expectedVersion,
                        stream
                            .Select(@event => new NewStreamMessage(
                                Deterministic.Create(Deterministic.Namespaces.Events,
                                    $"{stream.Key}-{expectedVersion++}"),
                                typeMapping.GetEventName(@event.GetType()),
                                JsonConvert.SerializeObject(@event, eventSettings),
                                JsonConvert.SerializeObject(new Dictionary<string, string>
                                {
                                    {"$version", "0"}
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

        private static bool UseLocalLegacyStream(IConfiguration root)
        {
            return bool.TryParse(root[USE_LOCAL_FILE], out var useLocalFile) && useLocalFile;
        }

        private static Task<GetObjectResponse> GetS3LegacyStreamArchive(IConfiguration root)
        {
            var bucketName = root[LEGACY_STREAM_FILE_BUCKET];
            var file = root[LEGACY_STREAM_FILE_NAME];

            try
            {
                var s3Client = root
                    .GetAWSOptions()
                    .CreateServiceClient<IAmazonS3>();

                return s3Client.GetObjectAsync(
                    new GetObjectRequest
                    {
                        BucketName = bucketName,
                        Key = file
                    },
                    CancellationToken.None
                );

            }
            catch (Exception exception)
            {
                Console.WriteLine($"Error retrieving S3:{bucketName}/{file}: {exception}");
                return null;
            }

        }

        private static FileInfo GetLegacyStreamsArchiveInfo(IConfiguration root)
        {
            var directory = root[LEGACY_STREAM_FILE_DIRECTORY] ?? string.Empty;
            var fileName = root[LEGACY_STREAM_FILE_NAME] ?? string.Empty;
            var filePath = Path.Combine(directory, fileName);

            try
            {
                var legacyStreamsArchiveInfo = new FileInfo(filePath);
                if(false == legacyStreamsArchiveInfo.Exists)
                    Console.WriteLine($"Import file '{legacyStreamsArchiveInfo.FullName}' does not exist");

                return legacyStreamsArchiveInfo;
            }
            catch
            {
                Console.WriteLine($"Import file path '{filePath}' is not valid");
                return null;
            }
        }
    }
}
