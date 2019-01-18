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
    using SqlStreamStore;
    using SqlStreamStore.Streams;
    using System.Linq;
    using System.Collections.Concurrent;
    using System.Threading;
    using Amazon.S3;
    using Amazon.S3.Model;
    using BackOffice.Messages;


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
            var connectionString = root.GetConnectionString("StreamStore");

            var masterConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString)
                {
                    InitialCatalog = "master",
                    ConnectRetryCount = 120,
                    ConnectRetryInterval = 1
                };

            await WaitForSqlServer(masterConnectionStringBuilder);

            using(var connection = new SqlConnection(masterConnectionStringBuilder.ConnectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);

                using(var command = new SqlCommand(
@"IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'RoadRegistry')
BEGIN
    CREATE DATABASE [RoadRegistry]
END",
                    connection))
                {
                    await command.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

            // Attempt to reconnect every 30 seconds, for an hour - could be set in the connection string as well.
            var connectionStringBuilder =
                new SqlConnectionStringBuilder(connectionString)
                {
                    ConnectRetryCount = 120,
                    ConnectRetryInterval = 30
                };
            using (var streamStore = new MsSqlStreamStore(new MsSqlStreamStoreSettings(connectionStringBuilder.ConnectionString)
            {
                Schema = "RoadRegistry"
            }))
            {
                await streamStore.CreateSchema().ConfigureAwait(false);

                var page = await streamStore.ReadStreamForwards("roadnetwork", StreamVersion.Start, 1).ConfigureAwait(false);
                if (page.Status == PageReadStatus.StreamNotFound)
                    await ImportStreams(root, streamStore);
                else
                    Console.WriteLine("Cannot import in an existing RoadNetwork. Aborted import");
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
                foreach (var batch in reader.Read(getEventStream).Batch(1000))
                {
                    foreach (var stream in batch.GroupBy(item => item.Stream, item => item.Event))
                    {
                        if (!expectedVersions.TryGetValue(stream.Key, out var expectedVersion))
                        {
                            expectedVersion = ExpectedVersion.NoStream;
                        }

                        innerWatch.Restart();

                        var appendResult = await streamStore.AppendToStream(
                            stream.Key,
                            expectedVersion,
                            stream
                                .Select(@event => new NewStreamMessage(
                                    Deterministic.Create(Deterministic.Namespaces.Events,$"{stream.Key}-{expectedVersion++}"),
                                    typeMapping.GetEventName(@event.GetType()),
                                    JsonConvert.SerializeObject(@event, eventSettings),
                                    JsonConvert.SerializeObject(new Dictionary<string, string>{ {"$version", "0"} }, eventSettings)
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
            else
            {
                Console.WriteLine("No event stream found");
            }
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
                Console.WriteLine($"Error downloading S3:{bucketName}/{file}: {exception}");
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

        private static async Task WaitForSqlServer(SqlConnectionStringBuilder builder, CancellationToken token = default)
        {
            var exit = false;
            while(!exit)
            {
                try
                {
                    using (var connection = new SqlConnection(builder.ConnectionString))
                    {
                        await connection.OpenAsync(token).ConfigureAwait(false);
                        exit = true;
                    }
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
